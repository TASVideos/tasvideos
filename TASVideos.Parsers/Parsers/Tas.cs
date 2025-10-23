using System.Security.Cryptography;
using System.Text;
using SharpCompress.Common;

namespace TASVideos.MovieParsers.Parsers;

[FileExtension("tas")]
internal class Tas : Parser, IParser
{
	private const int FrameRate = 60;

	private const string FileTimeHeader = "FileTime:";
	private const string ChapterTimeHeader = "ChapterTime:";
	private const string TotalRerecordCountHeader = "TotalRecordCount:";
	private const string RerecordCountHeader = "RecordCount:";

	public async Task<IParseResult> Parse(Stream file, long length)
	{
		var result = new SuccessResult(FileExtension)
		{
			Region = RegionType.Ntsc,
			SystemCode = SystemCodes.Celeste,
			FrameRateOverride = 58.823529411764705
		};

		var fileTimeFound = 0;
		var chapterTimeFound = 0;
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

				var split = s.Split(['(', ')']);
				if (split.Length == 3)
				{
					var test = int.TryParse(split[1], out int igtFrames);
					if (test)
					{
						result.Frames = igtFrames;
					}
				}
				else
				{
					return Error("FileTime did not meet expected format");
				}
			}

			if (fileTimeFound == 0 && s.StartsWith(ChapterTimeHeader))
			{
				chapterTimeFound = 1;
				if (string.IsNullOrWhiteSpace(s))
				{
					return Error("No ChapterTime duration found");
				}

				var split = s.Split(['(', ')']);
				if (split.Length == 3)
				{
					var test = int.TryParse(split[1], out int igtFrames);
					if (test)
					{
						result.Frames = igtFrames;
					}
				}
				else
				{
					return Error("ChapterTime did not meet expected format");
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

		if (fileTimeFound == 0 && chapterTimeFound == 0)
		{
			return Error("No FileTime or ChapterTime found, cannot parse");
		}

		return result;
	}
}
