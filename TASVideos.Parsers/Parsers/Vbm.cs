using TASVideos.MovieParsers.Extensions;
using TASVideos.MovieParsers.Result;

namespace TASVideos.MovieParsers.Parsers;

[FileExtension("vbm")]
internal class Vbm : ParserBase, IParser
{
	public override string FileExtension => "vbm";

	public async Task<IParseResult> Parse(Stream file)
	{
		var result = new ParseResult
		{
			Region = RegionType.Ntsc,
			FileExtension = FileExtension,
			SystemCode = SystemCodes.GameBoy
		};

		using var br = new BinaryReader(file);
		var header = new string(br.ReadChars(4));
		if (!header.StartsWith("VBM"))
		{
			return new ErrorResult("Invalid file format, does not seem to be a .vbm");
		}

		br.ReadBytes(8); // major version, movie uid
		result.Frames = br.ReadInt32();
		result.RerecordCount = br.ReadInt32();

		var type = br.ReadByte();
		if (type.Bit(0))
		{
			result.StartType = MovieStartType.Savestate;
		}
		else if (type.Bit(1))
		{
			result.StartType = MovieStartType.Sram;
		}

		br.ReadByte(); // Controller config
		var system = br.ReadByte();
		if (system.Bit(0))
		{
			result.SystemCode = SystemCodes.Gba;
		}
		else if (system.Bit(1))
		{
			result.SystemCode = SystemCodes.Gbc;
		}
		else if (system.Bit(2))
		{
			result.SystemCode = SystemCodes.Sgb;
		}
		else
		{
			result.SystemCode = SystemCodes.GameBoy;
		}

		return await Task.FromResult(result);
	}
}
