using System.IO;

using TASVideos.MovieParsers.Extensions;
using TASVideos.MovieParsers.Result;


namespace TASVideos.MovieParsers.Parsers
{
	[FileExtension("mar")]
	internal class Mar : ParserBase, IParser
	{
		public override string FileExtension => "mar";

		public IParseResult Parse(Stream file)
		{
			var result = new ParseResult
			{
				Region = RegionType.Ntsc,
				FileExtension = FileExtension,
				SystemCode = SystemCodes.Arcade
			};

			using (var br = new BinaryReader(file))
			{
				var header = new string(br.ReadChars(8));
				if (!header.StartsWith("MAMETAS\0"))
				{
					return new ErrorResult("Invalid file format, does not seem to be a .mar");
				}
			}

			return result;
		}
	}
}
