using TASVideos.MovieParsers.Result;

namespace TASVideos.MovieParsers.Parsers;

[FileExtension("mar")]
internal class Mar : ParserBase, IParser
{
	public override string FileExtension => "mar";

	public async Task<IParseResult> Parse(Stream file)
	{
		var result = new ParseResult
		{
			Region = RegionType.Ntsc,
			FileExtension = FileExtension,
			SystemCode = SystemCodes.Arcade
		};

		using var br = new BinaryReader(file);
		var header = new string(br.ReadChars(8));
		if (!header.StartsWith("MAMETAS\0"))
		{
			return new ErrorResult("Invalid file format, does not seem to be a .mar");
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
