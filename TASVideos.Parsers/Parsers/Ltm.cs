using SharpCompress.Readers;
using TASVideos.MovieParsers.Result;

namespace TASVideos.MovieParsers.Parsers;

[FileExtension("ltm")]
internal class Ltm : ParserBase, IParser
{
	public const double DefaultFrameRate = 60.0;

	private const string FrameCountHeader = "frame_count=";
	private const string RerecordCountHeader = "rerecord_count=";
	private const string SaveStateCountHeader = "savestate_frame_count=";
	private const string FrameRateDenHeader = "framerate_den=";
	private const string FrameRateNumHeader = "framerate_num";

	public override string FileExtension => "ltm";

	public async Task<IParseResult> Parse(Stream file)
	{
		var result = new ParseResult
		{
			Region = RegionType.Ntsc,
			FileExtension = FileExtension,
			SystemCode = SystemCodes.Linux
		};

		double? frameRateDenominator = null;
		double? frameRateNumerator = null;

		using (var reader = ReaderFactory.Open(file))
		{
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
						string? s;
						while ((s = await textReader.ReadLineAsync()) != null)
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
						}

						break;
					case "annotations.txt":
						string? line;
						while ((line = await textReader.ReadLineAsync()) != null)
						{
							if (line.ToLower().StartsWith("platform:"))
							{
								result.SystemCode = CalculatePlatform(GetPlatformValue(line));
							}
						}

						break;
				}

				entry.SkipEntry(); // seems to be required if the stream was not fully consumed
			}
		}

		if (frameRateDenominator > 0 && frameRateNumerator.HasValue)
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

		var split = str.Split(new[] { "=" }, StringSplitOptions.RemoveEmptyEntries);

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

		var split = str.Split(new[] { "=" }, StringSplitOptions.RemoveEmptyEntries);

		if (split.Length > 1)
		{
			var intStr = split.Skip(1).First();
			var result = double.TryParse(intStr, out double val);
			if (result)
			{
				return val;
			}
		}

		return 0;
	}

	private static string GetPlatformValue(string str)
	{
		if (string.IsNullOrWhiteSpace(str))
		{
			return "";
		}

		var split = str.ToLower().Split(new[] { "platform:" }, StringSplitOptions.RemoveEmptyEntries);
		if (split.Length != 1)
		{
			return "";
		}

		return split[0].Trim();
	}

	private static string CalculatePlatform(string str)
	{
		if (string.Equals(SystemCodes.Flash, str, StringComparison.InvariantCultureIgnoreCase))
		{
			return SystemCodes.Flash;
		}

		if (string.Equals(SystemCodes.Dos, str, StringComparison.InvariantCultureIgnoreCase))
		{
			return SystemCodes.Dos;
		}

		if (string.Equals(SystemCodes.Windows, str, StringComparison.InvariantCultureIgnoreCase))
		{
			return SystemCodes.Windows;
		}

		return SystemCodes.Linux;
	}
}
