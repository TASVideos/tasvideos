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
			if(m_magic != Magic) {
				return InvalidFormat();
			}

			reader.ReadUInt32();
			result.Frames = (int)reader.ReadUInt32();
			uint rngLen = reader.ReadUInt32();

			file.Seek(1024, SeekOrigin.Begin);

			for (int i = 0; i < result.Frames; i++)
			{
				reader.ReadUInt64();
			}
			for (int i = 0; i < rngLen; i++)
			{
				reader.ReadInt32();
				reader.ReadDouble();
			}
		}
		catch (System.IO.EndOfStreamException e)
		{
			return InvalidFormat();
		}

		return await Task.FromResult(result);
	}
}
