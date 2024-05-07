namespace TASVideos.MovieParsers.Parsers;

[FileExtension("m64")]
internal class M64 : Parser, IParser
{
	public async Task<IParseResult> Parse(Stream file, long length)
	{
		var result = new SuccessResult(FileExtension)
		{
			Region = RegionType.Ntsc,
			SystemCode = SystemCodes.N64
		};

		using var br = new BinaryReader(file);
		var header = new string(br.ReadChars(4));
		if (header != "M64\u001a")
		{
			return InvalidFormat();
		}

		br.ReadUInt32(); // Version id
		br.ReadInt32(); // Movie uid and unix time of recording
		result.Frames = (int)br.ReadUInt32(); // Vertical interrupts
		result.RerecordCount = (int)br.ReadUInt32();
		var fps = br.ReadByte(); // Vertical interrupts per second
		if (fps == 50)
		{
			result.Region = RegionType.Pal;
		}

		br.ReadByte(); // Number of controllers
		br.ReadUInt16(); // Reserved
		br.ReadUInt32(); // Number of input samples
		var type = br.ReadByte();
		if (type.Bit(0))
		{
			result.StartType = MovieStartType.Savestate;
		}
		else if (type.Bit(1))
		{
			result.StartType = MovieStartType.PowerOn;
		}
		else if (type.Bit(2))
		{
			result.StartType = MovieStartType.Sram;
		}

		return await Task.FromResult(result);
	}
}
