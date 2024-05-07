namespace TASVideos.MovieParsers.Parsers;

[FileExtension("fbm")]
internal class Fbm : Parser, IParser
{
	public async Task<IParseResult> Parse(Stream file, long length)
	{
		var result = new SuccessResult(FileExtension)
		{
			Region = RegionType.Ntsc,
			SystemCode = SystemCodes.Arcade
		};

		using var br = new BinaryReader(file);
		var header = new string(br.ReadChars(4));
		if (header != "FB1 ")
		{
			return InvalidFormat();
		}

		br.ReadByte(); // Version number
		var nextHeader = new string(br.ReadChars(4));
		if (nextHeader == "FS1 ")
		{
			result.StartType = MovieStartType.Savestate;
			br.ReadBytes(16);
			int stateLength = br.ReadInt32();
			br.ReadBytes(32); // Name of the game
			br.ReadBytes(4); // Number of frames before savestate
			br.ReadBytes(12); // Reserved
			br.ReadBytes(stateLength);
			nextHeader = new string(br.ReadChars(4));
		}

		if (nextHeader != "FR1 ")
		{
			return Error("Input data not found");
		}

		br.ReadBytes(4); // Size of frame data chunk in bytes (not including the chunk identifier)
		result.Frames = br.ReadInt32();
		result.RerecordCount = br.ReadInt32();

		return await Task.FromResult(result);
	}
}
