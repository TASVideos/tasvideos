using System.Text;

namespace TASVideos.MovieParsers.Parsers;

[FileExtension("p2m2")]
internal class P2m2 : Parser, IParser
{
	public async Task<IParseResult> Parse(Stream file, long length)
	{
		var result = new SuccessResult(FileExtension)
		{
			Region = RegionType.Ntsc,
			SystemCode = SystemCodes.Ps2
		};

		using var br = new BinaryReader(file);
		br.ReadBytes(1); // Recording File Version
		var header = new string(br.ReadChars(5));
		if (header != "PCSX2")
		{
			return InvalidFormat();
		}

		br.ReadBytes(2); // "-v"
		br.ReadBytes(43); // PCSX2 Version Used
		br.ReadBytes(255); // Author
		br.ReadBytes(255); // Associated Game Name or ISO Filename
		result.Frames = br.ReadInt32();
		result.RerecordCount = br.ReadInt32();
		var startsFromSavestate = br.ReadByte() > 0;
		if (startsFromSavestate)
		{
			result.StartType = MovieStartType.Savestate;
		}

		return await Task.FromResult(result);
	}
}
