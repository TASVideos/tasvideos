using TASVideos.MovieParsers.Result;

namespace TASVideos.MovieParsers.Parsers;

[FileExtension("dtm")]
internal class Dtm : ParserBase, IParser
{
	private const decimal GameCubeHertz = 486000000.0M;
	private const decimal WiiHertz = 729000000.0M;
	public override string FileExtension => "dtm";

	public async Task<IParseResult> Parse(Stream file, long length)
	{
		var result = new ParseResult
		{
			Region = RegionType.Ntsc,
			FileExtension = FileExtension,
			SystemCode = SystemCodes.GameCube
		};

		using var br = new BinaryReader(file);
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
		br.ReadByte(); // PAL60 setting (this setting only applies to Wii games that support both 50 Hz and 60 Hz)
		br.ReadBytes(12); // Reserved
		br.ReadBytes(40); // Name of second disc iso
		br.ReadBytes(20); // SHA-1 has of git revision
		br.ReadBytes(4); // DSP
		br.ReadBytes(4); // DSP
		result.CycleCount = br.ReadInt64(); // (486 MHz when a GameCube game is running, 729 MHz when a Wii game is running)
		if (result.CycleCount != 0)
		{
			var hertz = isWii ? WiiHertz : GameCubeHertz;
			result.Frames = (int)Math.Ceiling((decimal)result.CycleCount / hertz * 60.0M);
		}
		else
		{
			result.WarnLengthInferred();
		}

		return await Task.FromResult(result);
	}
}
