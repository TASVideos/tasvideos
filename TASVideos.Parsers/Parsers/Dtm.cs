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

				br.ReadChars(6); // Game Id
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
				br.ReadInt64(); // Lag count
				br.ReadInt64(); // Reserved
				result.RerecordCount = br.ReadInt32();
				br.ReadBytes(32); // Author
				br.ReadBytes(16); // Video backend
				br.ReadBytes(16); // Audio Emulator
				br.ReadBytes(16); // Md5
				br.ReadBytes(8); // Recording start time
				br.ReadBytes(14); // Various flags
				var hasMemoryCards = br.ReadByte() > 0;
				var memoryCardBlank = br.ReadByte() > 0;
				if (hasMemoryCards && !memoryCardBlank)
				{
					result.StartType = MovieStartType.Sram;
				}
			}


			return result;
		}
	}
}
