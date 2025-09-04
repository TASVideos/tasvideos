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
		if (!header.StartsWith("PCSX2"))
		{
			return InvalidFormat();
		}

		br.ReadBytes(2); // "-v"
		br.ReadBytes(8); // PCSX2 Version Used
		br.ReadBytes(35);
		br.ReadBytes(32); // Author
		br.ReadBytes(223);
		br.ReadBytes(32); // Associated Game Name or ISO Filename
		br.ReadBytes(223);
		result.Frames = br.ReadUInt16();
		br.ReadBytes(2);
		result.RerecordCount = br.ReadUInt16();
		br.ReadBytes(2);
		var startsFromSavestate = br.ReadByte() > 0;
		if (startsFromSavestate)
		{
			result.StartType = MovieStartType.Savestate;
		}

		return await Task.FromResult(result);
	}
}
