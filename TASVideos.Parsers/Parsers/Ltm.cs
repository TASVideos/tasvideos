using System.Text;
using SharpCompress.Readers;

namespace TASVideos.MovieParsers.Parsers;

[FileExtension("ltm")]
internal class Ltm : Parser, IParser
{
	public const double DefaultFrameRate = 60.0;

	private const string FrameCountHeader = "frame_count=";
	private const string RerecordCountHeader = "rerecord_count=";
	private const string SaveStateCountHeader = "savestate_frame_count=";
	private const string FrameRateDenHeader = "framerate_den=";
	private const string FrameRateNumHeader = "framerate_num=";
	private const string GameNameHeader = "game_name=";
	private const string VariableFramerateHeader = "variable_framerate=";
	private const string LengthSecondsHeader = "length_sec=";
	private const string LengthNanosecondsHeader = "length_nsec=";
	private const string Md5 = "md5=";
	public async Task<IParseResult> Parse(Stream file, long length)
	{
		var result = new SuccessResult(FileExtension)
		{
			Region = RegionType.Ntsc,
			SystemCode = SystemCodes.Linux
		};

		double? frameRateDenominator = null;
		double? frameRateNumerator = null;
		double? lengthSeconds = null;
		double? lengthNanoseconds = null;
		bool isVariableFramerate = false;

		using var reader = ReaderFactory.Open(file);
		while (reader.MoveToNextEntry())
		{
			if (reader.Entry.IsDirectory)
			{
				continue;
			}

			await using var entry = reader.OpenEntryStream();
			using var textReader = new StreamReader(entry);
			switch (reader.Entry.Key)
			{
				case "config.ini":
					while (await textReader.ReadLineAsync() is { } s)
					{
						if (s.StartsWith(FrameCountHeader))
						{
							result.Frames = ParseIntFromConfig(s);
						}
						else if (s.StartsWith(RerecordCountHeader))
						{
							result.RerecordCount = ParseIntFromConfig(s);
						}
						else if (s.StartsWith(SaveStateCountHeader))
						{
							var savestateCount = ParseIntFromConfig(s);

							// Power-on movies seem to always have a savestate count equal to frames
							if (savestateCount > 0 && savestateCount != result.Frames)
							{
								result.StartType = MovieStartType.Savestate;
							}
						}
						else if (s.StartsWith(FrameRateDenHeader))
						{
							frameRateDenominator = ParseDoubleFromConfig(s);
						}
						else if (s.StartsWith(FrameRateNumHeader))
						{
							frameRateNumerator = ParseDoubleFromConfig(s);
						}
						else if (s.StartsWith(GameNameHeader))
						{
							var gameName = ParseStringFromConfig(s);

							if (gameName.Contains("ruffle", StringComparison.OrdinalIgnoreCase))
							{
								result.SystemCode = SystemCodes.Flash;
							}
						}
						else if (s.StartsWith(VariableFramerateHeader))
						{
							isVariableFramerate = ParseBoolFromConfig(s);
						}
						else if (s.StartsWith(LengthSecondsHeader))
						{
							lengthSeconds = ParseDoubleFromConfig(s);
						}
						else if (s.StartsWith(LengthNanosecondsHeader))
						{
							lengthNanoseconds = ParseDoubleFromConfig(s);
						}
						else if (s.StartsWith(Md5))
						{
							var md5 = ParseStringFromConfig(s);
							if (md5.Length == 32)
							{
								result.Hashes.Add(HashType.Md5, md5.ToLower());
							}
						}
					}

					break;
				case "annotations.txt":
					var sb = new StringBuilder();
					while (await textReader.ReadLineAsync() is { } line)
					{
						if (line.StartsWith("platform:", StringComparison.InvariantCultureIgnoreCase))
						{
							result.SystemCode = CalculatePlatform(GetPlatformValue(line));
						}
						else
						{
							sb.AppendLine(line);
						}
					}

					result.Annotations = sb.ToString();
					break;
			}

			entry.SkipEntry(); // seems to be required if the stream was not fully consumed
		}

		if (isVariableFramerate)
		{
			result.FrameRateOverride = result.Frames / (lengthSeconds + (lengthNanoseconds / 1000000000.0D));
		}
		else if (frameRateDenominator > 0 && frameRateNumerator.HasValue)
		{
			result.FrameRateOverride = frameRateNumerator / frameRateDenominator;
		}
		else
		{
			result.WarnNoFrameRate();
			result.FrameRateOverride = DefaultFrameRate;
		}

		return result;
	}

	private static int ParseIntFromConfig(string str)
	{
		if (string.IsNullOrWhiteSpace(str))
		{
			return 0;
		}

		var split = str.SplitWithEmpty("=");
		if (split.Length > 1)
		{
			var intStr = split.Skip(1).First();
			var result = int.TryParse(intStr, out int val);
			if (result)
			{
				return val;
			}
		}

		return 0;
	}

	private static double ParseDoubleFromConfig(string str)
	{
		if (string.IsNullOrWhiteSpace(str))
		{
			return 0;
		}

		var split = str.SplitWithEmpty("=");
		if (split.Length > 1)
		{
			var doubleStr = split.Skip(1).First();
			var result = double.TryParse(doubleStr, NumberStyles.Float | NumberStyles.AllowThousands, NumberFormatInfo.InvariantInfo, out double val);
			if (result)
			{
				return val;
			}
		}

		return 0;
	}

	private static string ParseStringFromConfig(string str)
	{
		if (string.IsNullOrWhiteSpace(str))
		{
			return string.Empty;
		}

		var split = str.SplitWithEmpty("=");
		if (split.Length > 1)
		{
			return split[1].Trim();
		}

		return string.Empty;
	}

	private static bool ParseBoolFromConfig(string str)
	{
		if (string.IsNullOrWhiteSpace(str))
		{
			return false;
		}

		var split = str.SplitWithEmpty("=");
		if (split.Length <= 1)
		{
			return false;
		}

		var boolStr = split.Skip(1).First();
		var result = bool.TryParse(boolStr, out bool val);
		return result && val;
	}

	private static string GetPlatformValue(string str)
	{
		if (string.IsNullOrWhiteSpace(str))
		{
			return "";
		}

		var split = str.ToLower().SplitWithEmpty("platform:");
		return split.Length == 1 ? split[0].Trim().ToLowerInvariant() : "";
	}

	private static string CalculatePlatform(string str)
	{
		if (typeof(SystemCodes).GetFields().Select(f => f.GetValue(f)).Contains(str))
		{
			return str;
		}

		return SystemCodes.Linux;
	}
}
