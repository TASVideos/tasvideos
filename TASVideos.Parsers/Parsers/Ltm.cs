using System;
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

		public IParseResult Parse(Stream file)
		{
			var result = new ParseResult
			{
				Region = RegionType.Ntsc,
				FileExtension = FileExtension
			};

			using (var reader = ReaderFactory.Open(file))
			{
				var blah = reader.OpenEntryStream();
				int zzz = 0;
			}

			return result;
		}
	}
}