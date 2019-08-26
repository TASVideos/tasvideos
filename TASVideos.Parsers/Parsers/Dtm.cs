using System.IO;
using TASVideos.MovieParsers.Result;

namespace TASVideos.MovieParsers.Parsers
{
	[FileExtension("dtm")]
	internal class Dtm : ParserBase, IParser
	{
		public override string FileExtension => "dtm";

		public IParseResult Parse(Stream file)
		{
			var result = new ParseResult
			{
				Region = RegionType.Ntsc,
				FileExtension = FileExtension,
				SystemCode = SystemCodes.GameCube
			};

			using (var br = new BinaryReader(file))
			{
				var header = new string(br.ReadChars(4));
				if (header != "DTM\u001a")
				{
					return new ErrorResult("Invalid file format, does not seem to be a .dtm");
				}

				br.ReadChars(6); // Game Id, not used
				var isWii = br.ReadByte() > 0;
				if (isWii)
				{
					result.SystemCode = SystemCodes.Wii;
				}

				br.ReadByte(); // Controller config, not used
				var startsFromSavestate = br.ReadByte() > 0;
				if (startsFromSavestate)
				{
					result.StartType = MovieStartType.Savestate;
				}

				var viCount = br.ReadInt64();
				var inputCount = br.ReadInt64();
				br.ReadInt64(); // Lag count, not used
				br.ReadInt64(); // Reserved, not used
				result.RerecordCount = br.ReadInt32();
			}


			return result;
		}
	}
}
