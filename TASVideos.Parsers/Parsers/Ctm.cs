using TASVideos.MovieParsers.Result;

namespace TASVideos.MovieParsers.Parsers;

[FileExtension("ctm")]
internal class Ctm : ParserBase, IParser
{
	private const decimal FrameRate = 268111856.0M / 4481136.0M; // https://github.com/citra-emu/citra/blob/a2f34ea82b5a31a7e842d0099921b85b8bce403f/src/core/hw/gpu.h#L27
	private const int InputRate = 234; // Rate at which inputs are polled per second
	public override string FileExtension => "ctm";

	public async Task<IParseResult> Parse(Stream file, long length)
	{
		var result = new ParseResult
		{
			Region = RegionType.Ntsc,
			FileExtension = FileExtension,
			SystemCode = SystemCodes.N3ds,
		};

		using var br = new BinaryReader(file);
		var header = new string(br.ReadChars(4));
		if (header != "CTM\x1b")
		{
			return new ErrorResult("Invalid file format, does not seem to be a .ctm");
		}

		br.ReadUInt64(); // Title ID
		br.ReadBytes(20); // Git hash of Citra revision
		br.ReadUInt64(); // Init time of system clock
		br.ReadUInt64(); // Movie ID
		br.ReadChars(32); // Author
		result.RerecordCount = br.ReadInt32();
		result.Frames = (int)Math.Ceiling((decimal)br.ReadUInt64() / InputRate * FrameRate);

		return await Task.FromResult(result);
	}
}
