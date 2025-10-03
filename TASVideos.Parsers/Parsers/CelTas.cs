using System.Security.Cryptography;
using System.Text;
using SharpCompress.Common;

namespace TASVideos.MovieParsers.Parsers;

[FileExtension("celtas")]
internal class CelTas : Parser, IParser
{
	private const int FrameRate = 60;
	private const int IGTFrameRate = 1000;

	private const string FileTimeHeader = "FileTime: ";
	private const string TotalRerecordCountHeader = "TotalRecordCount: ";
	private const string RerecordCountHeader = "RecordCount: ";

	public async Task<IParseResult> Parse(Stream file, long length)
	{
		var result = new SuccessResult(FileExtension)
		{
			Region = RegionType.Ntsc,
			SystemCode = SystemCodes.Celeste
		};

		var fileTimeFound = 0;
		var totalRecordCountUsed = false;

		using var reader = new StreamReader(file);
		while (await reader.ReadLineAsync() is { } s)
		{
			if (s.StartsWith(FileTimeHeader))
			{
				fileTimeFound = 1;
				if (string.IsNullOrWhiteSpace(s))
				{
					return Error("No FileTime duration found");
				}

				var split = s.Split([':', '.', ' ', '(']);
				if (split.Length > 5) // This should always output at least 6 strings
				{
					var i = split.Length - 2;
					var test = int.TryParse(split[i], out int milliseconds);
					if (test)
					{
						result.Frames = milliseconds;
						i--;
					}

					test = int.TryParse(split[i], out int seconds);
					if (test)
					{
						result.Frames += seconds * 1000;
						i--;
					}

					test = int.TryParse(split[i], out int minutes);
					if (test)
					{
						result.Frames += minutes * 1000 * 60;
						i--;
					}

					if (i > 1)
					{
						test = int.TryParse(split[i], out int hours);
						if (test)
						{
							result.Frames += hours * 1000 * 60 * 60;
						}
					}
				}
				else
				{
					return Error("FileTime did not meet expected format");
				}
			}

			if (s.StartsWith(TotalRerecordCountHeader))
			{
				var split = s.SplitWithEmpty(":");
				if (split.Length > 1)
				{
					var intStr = split.Skip(1).First();
					var test = int.TryParse(intStr, out int val);
					if (test)
					{
						totalRecordCountUsed = true;
						result.RerecordCount = val;
					}
				}
			}

			if (s.StartsWith(RerecordCountHeader) && !totalRecordCountUsed)
			{
				if (string.IsNullOrWhiteSpace(s))
				{
					result.WarnNoRerecords();
				}

				var split = s.SplitWithEmpty(":");
				if (split.Length > 1)
				{
					var intStr = split.Skip(1).First();
					var test = int.TryParse(intStr, out int val);
					if (test)
					{
						result.RerecordCount = val;
					}
				}
				else
				{
					result.WarnNoRerecords();
				}
			}
		}

		if (fileTimeFound == 0)
		{
			return Error("No FileTime found, cannot parse");
		}

		return result;
	}
}
