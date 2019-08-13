using System;
using System.IO;
using System.Linq;
using SharpCompress.Readers;
using TASVideos.MovieParsers.Result;

namespace TASVideos.MovieParsers.Parsers
{
	[FileExtension("ltm")]
	internal class Ltm : ParserBase, IParser
	{
		private const string FrameCountHeader = "frame_count";
		private const string RerecordCountHeader = "rerecord_count";
		private const string SaveStateCountHeader = "savestate_frame_count";

		public override string FileExtension => "ltm";

		public IParseResult Parse(Stream file)
		{
			var result = new ParseResult
			{
				Region = RegionType.Ntsc,
				FileExtension = FileExtension,
				SystemCode = SystemCodes.Linux
			};

			using (var reader = ReaderFactory.Open(file))
			{
				while (reader.MoveToNextEntry())
				{
					if (reader.Entry.IsDirectory)
					{
						continue;
					}

					using (var entry = reader.OpenEntryStream())
					using (var textReader = new StreamReader(entry))
					{
						switch (reader.Entry.Key)
						{
							case "config.ini":
								while (textReader.ReadLine() is string s)
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
								}

								break;
						}

						entry.SkipEntry(); // seems to be required if the stream was not fully consumed
					}
				}
			}

			return result;
		}

		private int ParseIntFromConfig(string str)
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
	}
}