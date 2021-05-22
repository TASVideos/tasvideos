using System.IO;
using System.Threading.Tasks;
using TASVideos.Extensions;
using TASVideos.MovieParsers.Extensions;
using TASVideos.MovieParsers.Result;

namespace TASVideos.MovieParsers.Parsers
{
	[FileExtension("jrsr")]
	public class Jrsr : IParser
	{
		private const string FileExtension = "jrsr";
		public async Task<IParseResult> Parse(Stream file)
		{
			using var reader = new StreamReader(file);
			var result = new ParseResult
			{
				Region = RegionType.Ntsc,
				FileExtension = FileExtension,
				SystemCode = SystemCodes.Dos
			};

			var lines = (await reader.ReadToEndAsync()).LineSplit();

			if (lines.Length == 0)
			{
				return new ErrorResult("File is empty.");
			}

			if (lines[0] != "JRSR")
			{
				return new ErrorResult("Invalid .jrsr file.");
			}

			bool hasHeader = false;
			bool beginHeader = false;
			bool beginEvents = false;

			string lastInputLine = "";

			foreach (var line in lines)
			{
				if (line == "!BEGIN header")
				{
					beginHeader = true;
					hasHeader = true;
				}
				else if (line == "!BEGIN events")
				{
					beginHeader = false;
					beginEvents = true;
				}
				else if (line == "!BEGIN savestate")
				{
					return new ErrorResult("File contains a savestate");
				}
				else if (line.StartsWith("!BEGIN")) // DiskInfo and possibly other header sections
				{
					beginHeader = false;
					beginEvents = false;
				}
				else if (beginHeader)
				{
					if (line.StartsWith(Keys.RerecordCount))
					{
						result.RerecordCount = line.GetValue().ToInt() ?? 0;
					}
					else if (line.StartsWith(Keys.StartsFromSavestate))
					{
						result.StartType = MovieStartType.Savestate;
					}
				}
				else if (beginEvents)
				{
					if (line.StartsWith("+"))
					{
						lastInputLine = line;
					}
				}
			}

			if (!hasHeader)
			{
				return new ErrorResult("No header found");
			}

			if (!string.IsNullOrWhiteSpace(lastInputLine))
			{
				var nanoSeconds = GetTime(lastInputLine);

				if (nanoSeconds.HasValue)
				{
					result.Frames = (int)(nanoSeconds.Value / 16666667);
					result.FrameRateOverride = result.Frames / (nanoSeconds / 1000000000L);
				}
			}

			return result;
		}

		internal static long? GetTime(string str)
		{
			if (string.IsNullOrWhiteSpace(str))
			{
				return null;
			}

			if (!str.StartsWith("+"))
			{
				return null;
			}

			var split = str.SplitWithEmpty(" ");
			var lineStr = split[0].Trim('+');

			var result = long.TryParse(lineStr, out long val);
			if (result)
			{
				return val;
			}

			return null;
		}

		private static class Keys
		{
			public const string RerecordCount = "+RERECORDS";
			public const string StartsFromSavestate = "!SAVESTATEID";
		}
	}
}
