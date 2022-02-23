using TASVideos.MovieParsers.Result;

namespace TASVideos.MovieParsers.Parsers;

[FileExtension("ctm")]
internal class Ctm : ParserBase, IParser
{
	private const decimal FrameRate = 59.83122493939037M;
	private const int CycleRate = 234;
	public override string FileExtension => "ctm";

	public async Task<IParseResult> Parse(Stream file)
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
		result.CycleCount = br.ReadInt64();
		result.Frames = (int)Math.Ceiling((decimal)result.CycleCount / CycleRate * FrameRate);

		return await Task.FromResult(result);
	}
}
