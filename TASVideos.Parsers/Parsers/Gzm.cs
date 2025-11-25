using System.Buffers.Binary;

namespace TASVideos.MovieParsers.Parsers;

[FileExtension("gzm")]
internal class Gzm : Parser, IParser
{
	private const decimal FrameRate = 60;
	public async Task<IParseResult> Parse(Stream file, long length)
	{
		var result = new SuccessResult(FileExtension)
		{
			Region = RegionType.Ntsc,
			SystemCode = SystemCodes.N64,
			FrameRateOverride = 20
		};

		using var reader = new BinaryReader(file);
		try
		{
			result.Frames = (int)BinaryPrimitives.ReverseEndianness(reader.ReadUInt32());
			int n_seed = (int)BinaryPrimitives.ReverseEndianness(reader.ReadUInt32());

			reader.ReadUInt16();
			reader.ReadByte();
			reader.ReadByte();

			for (int i = 0; i < result.Frames; i++)
			{
				reader.ReadUInt16();
				reader.ReadByte();
				reader.ReadByte();
				reader.ReadUInt16();
			}

			for (int j = 0; j < n_seed; j++)
			{
				reader.ReadInt32();
				reader.ReadUInt32();
				reader.ReadUInt32();
			}

			int n_oca_input = (int)BinaryPrimitives.ReverseEndianness(reader.ReadUInt32());
			int n_oca_sync = (int)BinaryPrimitives.ReverseEndianness(reader.ReadUInt32());
			int n_room_load = (int)BinaryPrimitives.ReverseEndianness(reader.ReadUInt32());

			for (int a = 0; a < n_oca_input; a++)
			{
				reader.ReadBytes(8);
			}

			for (int a = 0; a < n_oca_sync; a++)
			{
				reader.ReadBytes(8);
			}

			for (int a = 0; a < n_room_load; a++)
			{
				reader.ReadBytes(4);
			}

			result.RerecordCount = (int)BinaryPrimitives.ReverseEndianness(reader.ReadUInt32());
			int lastframe = (int)BinaryPrimitives.ReverseEndianness(reader.ReadUInt32());
		}
		catch (System.IO.EndOfStreamException e)
		{
			return Error("Misformatted file");
		}

		// check we hit the end of the file
		if(reader.PeekChar() != -1)
		{
			return InvalidFormat();
		}

		return await Task.FromResult(result);
	}
}
