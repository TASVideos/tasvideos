namespace TASVideos.MovieParsers.Parsers;

[FileExtension("ctas")]
internal class Ctas : Parser, IParser
{
	private const double FrameRate = 60;
	private const uint Magic = 0x53415443;
	public async Task<IParseResult> Parse(Stream file, long length)
	{
		var result = new SuccessResult(FileExtension)
		{
			Region = RegionType.Ntsc,
			SystemCode = SystemCodes.Pc,
			FrameRateOverride = FrameRate
		};

		using var reader = new BinaryReader(file);
		try
		{
			var magic = reader.ReadUInt32();
			if (magic != Magic)
			{
				return InvalidFormat();
			}

			var version = reader.ReadUInt32();
			var framecount = reader.ReadUInt32();
			var rngLen = reader.ReadUInt32();
			uint reportedTime = 0;

			if (version >= 4)
			{
				result.RerecordCount = (int)reader.ReadUInt32();
				reportedTime = reader.ReadUInt32();
				result.Frames = (int)(reportedTime / (1000 / 60));
				byte[] buf = new byte[1000];

				reader.Read(buf);
			}
			else
			{
				byte[] buf = new byte[1008];

				reader.Read(buf);
			}

			for (int i = 0; i < framecount; i++)
			{
				reader.ReadUInt64();
			}

			for (int i = 0; i < rngLen; i++)
			{
				reader.ReadInt32();
				reader.ReadDouble();
			}

			if (reportedTime <= 0)
			{
				result.Frames = (int)framecount;
			}
		}
		catch (System.IO.EndOfStreamException)
		{
			return InvalidFormat();
		}

		// check we hit the end of the file
		if(reader.PeekChar() != -1)
		{
			return InvalidFormat();
		}

		return await Task.FromResult(result);
	}
}
