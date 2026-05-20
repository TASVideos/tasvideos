using System.Buffers.Binary;

namespace TASVideos.MovieParsers.Parsers;

[FileExtension("ctas")]
internal class CTas : Parser, IParser
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
			if(m_magic != Magic)
			{
				return InvalidFormat();
			}

			uint version = reader.ReadUInt32();
			uint framecount = reader.ReadUInt32();
			uint rngLen = reader.ReadUInt32();
			uint reportedTime = 0;

			if (version >= 4)
			{
				result.RerecordCount = (int)reader.ReadUInt32();
				reportedTime = reader.ReadUInt32();
				result.Frames = (int)(reportedTime / (1000 / 60));
			}

			file.Seek(1024, SeekOrigin.Begin);

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

		return await Task.FromResult(result);
	}
}
