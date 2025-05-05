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
			RerecordCount = 1
		};

		return await Task.FromResult(result);
	}
}
