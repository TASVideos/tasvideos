namespace TASVideos.MovieParsers.Parsers;

[FileExtension("mar")]
internal class Mar : Parser, IParser
{
	public async Task<IParseResult> Parse(Stream file, long length)
	{
		var result = new SuccessResult(FileExtension)
		{
			Region = RegionType.Ntsc,
			SystemCode = SystemCodes.Arcade
		};

		using var br = new BinaryReader(file);
		var header = new string(br.ReadChars(8));
		if (!header.StartsWith("MAMETAS\0"))
		{
			return InvalidFormat();
		}

		br.ReadBytes(8);
		br.ReadBytes(32);
		var frameRate = br.ReadDouble();
		if (frameRate > 0)
		{
			result.FrameRateOverride = frameRate;
		}

		result.Frames = br.ReadInt32();
		result.RerecordCount = br.ReadInt32();

		return await Task.FromResult(result);
	}
}
