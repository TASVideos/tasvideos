using System.IO;
using System.IO.Compression;
using System.Linq;

using TASVideos.MovieParsers.Extensions;
using TASVideos.MovieParsers.Result;

namespace TASVideos.MovieParsers.Parsers
{
	[FileExtension("lsmv")]
	internal class Lsmv : ParserBase, IParser
	{
		private const string InputFile = "input";
		private const string RerecordFile = "rerecords";

		public override string FileExtension => "lsmv";

		public IParseResult Parse(Stream file)
		{
			var result = new ParseResult
			{
				Region = RegionType.Ntsc,
				FileExtension = FileExtension
			};

			var archive = new ZipArchive(file);

			var rerecordCountFile = archive.Entry(RerecordFile);
			if (rerecordCountFile != null)
			{
				using (var stream = rerecordCountFile.Open())
				{
					using (var reader = new StreamReader(stream))
					{
						var line = reader
							.ReadToEnd()
							.LineSplit()
							.FirstOrDefault();

						if (line == null)
						{
							result.WarnNoRerecords();
						}
						else
						{
							var parseResult = int.TryParse(line, out int rerecords);
							if (parseResult)
							{
								result.RerecordCount = rerecords;
							}
							else
							{
								result.WarnNoRerecords();
							}
						}
					}
				}
			}
			else
			{
				result.WarnNoRerecords();
			}

			var inputLog = archive.Entry(InputFile);
			if (inputLog == null)
			{
				return Error($"Missing {InputFile}, can not parse");
			}

			using (var stream = inputLog.Open())
			{
				using (var reader = new StreamReader(stream))
				{
					result.Frames = reader
						.ReadToEnd()
						.LineSplit()
						.Count(i => i.StartsWith("F"));
				}
			}

			return result;
		}
	}
}
