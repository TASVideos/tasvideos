namespace TASVideos.MovieParsers.Parsers;

[FileExtension("wtf")]
internal class Wtf : Parser, IParser
{
	public async Task<IParseResult> Parse(Stream file, long length)
	{
		var result = new SuccessResult(FileExtension)
		{
			Region = RegionType.Ntsc,
			SystemCode = SystemCodes.Windows,
			Frames = (int)((length - 1024) / 8)
		};

		using var br = new BinaryReader(file);
		var signature = br.ReadInt32(); // 0x66 0x54 0x77 0x02
		if (signature != 41374822)
		{
			return InvalidFormat();
		}

		br.ReadInt32(); // input frames
		result.RerecordCount = br.ReadInt32();
		br.ReadBytes(8); // keyboard type
		var fps = br.ReadUInt32();
		if (fps > 1)
		{
			result.FrameRateOverride = fps - 1;
		}

		return await Task.FromResult(result);
	}
}
