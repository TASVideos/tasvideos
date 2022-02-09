using TASVideos.MovieParsers.Result;

namespace TASVideos.MovieParsers.Parsers;

internal class Wtf : ParserBase, IParser
{
	public override string FileExtension => "wtf";

	public async Task<IParseResult> Parse(Stream file)
	{
		var result = new ParseResult
		{
			Region = RegionType.Ntsc,
			FileExtension = FileExtension,
			SystemCode = SystemCodes.Windows,
			Frames = (int)((file.Length - 1024) / 8)
		};

		using var br = new BinaryReader(file);
		var signature = br.ReadInt32(); // 0x66 0x54 0x77 0x02
		if (signature != 41374822)
		{
			return new ErrorResult("Invalid file format, does not seem to be a .wtf");
		}

		br.ReadInt32(); // input frames
		result.RerecordCount = br.ReadInt32();
		br.ReadBytes(8); // keyboard type
		var fps = br.ReadUInt32();
		if (fps > 0)
		{
			result.FrameRateOverride = fps;
		}

		return await Task.FromResult(result);
	}
}
