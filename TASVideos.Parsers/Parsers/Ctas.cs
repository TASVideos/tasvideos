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
			SystemCode = SystemCodes.Windows,
			FrameRateOverride = FrameRate
		};

		using var reader = new BinaryReader(file);
		try
		{
			uint m_magic = reader.ReadUInt32();
			if (m_magic != Magic)
			{
				return InvalidFormat();
			}

			reader.ReadUInt32();
			result.Frames = reader.ReadInt32();
			int rngLen = reader.ReadInt32();

			byte[] buf = new byte[1008];

			reader.Read(buf);

			for (int i = 0; i < result.Frames; i++)
			{
				reader.ReadUInt64();
			}

			for (int i = 0; i < rngLen; i++)
			{
				reader.ReadInt32();
				reader.ReadInt64();
			}
		}
		catch (System.IO.EndOfStreamException e)
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
