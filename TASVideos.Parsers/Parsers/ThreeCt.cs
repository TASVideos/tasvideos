namespace TASVideos.MovieParsers.Parsers;

[FileExtension("3ct")]
internal class ThreeCt : Parser, IParser
{
	public async Task<IParseResult> Parse(Stream file, long length)
	{
		var result = new SuccessResult(FileExtension)
		{
			Region = RegionType.Ntsc,
			SystemCode = SystemCodes.Nes,
			RerecordCount = 100 // Homage to the author, and because an arbitrary number is better than 0 to indicate the intention of no rerecord count
		};

		var lastLine = "";

		using var reader = new StreamReader(file);
		while (await reader.ReadLineAsync() is { } line)
		{
			lastLine = line;
		}

		var cycleCountStr = lastLine.Split(' ').FirstOrDefault();
		if (int.TryParse(cycleCountStr, out var cycleCount))
		{
			// The instructions suggest that the cartridge swap happens before this cycle executes,
			// so technically the "input" is the cycle before
			result.CycleCount = cycleCount - 1;
		}

		return result;
	}
}
