namespace TASVideos.MovieParsers.Parsers;

[FileExtension("gmv")]
internal class Gmv : Parser, IParser
{
	public async Task<IParseResult> Parse(Stream file, long length)
	{
		var result = new SuccessResult(FileExtension)
		{
			Region = RegionType.Ntsc,
			SystemCode = SystemCodes.Genesis
		};

		using var br = new BinaryReader(file);
		var header = new string(br.ReadChars(16));
		if (!header.StartsWith("Gens Movie"))
		{
			return InvalidFormat();
		}

		result.RerecordCount = br.ReadInt32();
		br.ReadBytes(2); // Controller config
		var flags = br.ReadByte();

		if (flags.Bit(7))
		{
			result.Region = RegionType.Pal;
		}

		if (flags.Bit(6))
		{
			result.StartType = MovieStartType.Savestate;
		}

		result.Frames = (int)(length - 64) / 3;

		return await Task.FromResult(result);
	}
}
