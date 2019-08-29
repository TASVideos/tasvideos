using System;
using System.IO;
using TASVideos.MovieParsers.Result;

namespace TASVideos.MovieParsers.Parsers
{
	[FileExtension("dtm")]
	internal class Dtm : ParserBase, IParser
	{
		private const int GameCubeHertz = 486000000;
		private const int WiiHertz = 729000000;
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

				result.Frames = (int)br.ReadInt64(); // Legacy .dtm format did not have ticks, so we need to fallback to vi count
				br.ReadInt64();
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

				br.ReadByte(); // Bongos
				br.ReadByte(); // Sync GPU
				br.ReadByte(); // Net play
				var isPal60 = br.ReadByte(); // SYSCONF PAL60 setting (this setting only applies to Wii games that support both 50 Hz and 60 Hz)
				br.ReadBytes(12); // Reserved
				br.ReadBytes(40); // Name of second disc iso
				br.ReadBytes(20); // SHA-1 has of git revision
				br.ReadBytes(4); // DSP IROM Hash
				br.ReadBytes(4); // DSP COEF Hash
				var ticks = br.ReadInt64(); // (486 MHz when a GameCube game is running, 729 MHz when a Wii game is running)
				if (ticks != 0)
				{
					var hertz = isWii ? WiiHertz : GameCubeHertz;
					result.Frames = (int)Math.Ceiling((decimal)(ticks / hertz * 60));
				}
				else
				{
					result.WarnLengthInferred();
				}
			}


			return result;
		}
	}
}
