using System;
using System.Diagnostics;
using System.IO;

using SharpCompress.Archives.Tar;
using SharpCompress.Common;
using SharpCompress.Compressors.Deflate;
using SharpCompress.Readers;
using SharpCompress.Readers.GZip;

using TASVideos.MovieParsers.Result;

namespace TASVideos.MovieParsers.Parsers
{
	[FileExtension("ltm")]
	internal class Ltm : ParserBase, IParser
	{
		public override string FileExtension => "ltm";

		private void DumpToConsole(TextReader r)
		{
			while (r.ReadLine() is string s)
			{
				Debug.WriteLine(s);
			}
		}

		public IParseResult Parse(Stream file)
		{
			var result = new ParseResult
			{
				Region = RegionType.Ntsc,
				FileExtension = FileExtension
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
								// framerate and such are in here
								Debug.WriteLine("##CONFIG##:");
								DumpToConsole(textReader);
								break;
							case "inputs":
								// also a text file, input roll stuff
								Debug.WriteLine("##INPUTS##:");
								DumpToConsole(textReader);
								break;
							default:
								break;
						}
						entry.SkipEntry(); // seems to be required if the stream was not fully consumed
					}
				}
			}

			return result;
		}
	}
}