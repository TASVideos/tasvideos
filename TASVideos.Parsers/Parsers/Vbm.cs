using System.IO;

using TASVideos.MovieParsers.Extensions;
using TASVideos.MovieParsers.Result;

namespace TASVideos.MovieParsers.Parsers
{
	[FileExtension("vbm")]
	internal class Vbm : ParserBase, IParser
	{
		public override string FileExtension => "vbm";

		public IParseResult Parse(Stream file)
		{
			var result = new ParseResult
			{
				Region = RegionType.Ntsc,
				FileExtension = FileExtension,
				SystemCode = SystemCodes.GameBoy
			};

			using (var br = new BinaryReader(file))
			{
				var header = new string(br.ReadChars(4));
				if (!header.StartsWith("VBM"))
				{
					return new ErrorResult("Invalid file format, does not seem to be a .vbm");
				}

				br.ReadBytes(8); // major version, movie uid
				result.Frames = br.ReadInt32();
				result.RerecordCount = br.ReadInt32();

				var type = br.ReadByte();
				if (type.Bit(0))
				{
					result.StartType = MovieStartType.Savestate;
				}
				else if (type.Bit(1))
				{
					result.StartType = MovieStartType.Sram;
				}
			}

			return result;
		}
	}
}
