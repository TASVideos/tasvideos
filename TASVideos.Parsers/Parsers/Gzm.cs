using System.Buffers.Binary;

namespace TASVideos.MovieParsers.Parsers;

[FileExtension("gzm")]
internal class Gzm : Parser, IParser
{
	private const double FrameRate = 60;
	public async Task<IParseResult> Parse(Stream file, long length)
	{
		var result = new SuccessResult(FileExtension)
		{
			Region = RegionType.Ntsc,
			SystemCode = SystemCodes.N64,
			FrameRateOverride = FrameRate
		};

		using var reader = new BinaryReader(file);
		try
		{
			var frameCount = (int)BinaryPrimitives.ReverseEndianness(reader.ReadUInt32());
			var seed = (int)BinaryPrimitives.ReverseEndianness(reader.ReadUInt32());

			reader.ReadUInt16();
			reader.ReadByte();
			reader.ReadByte();

			for (var i = 0; i < frameCount; i++)
			{
				reader.ReadUInt16();
				reader.ReadByte();
				reader.ReadByte();
				reader.ReadUInt16();
			}

			for (var j = 0; j < seed; j++)
			{
				reader.ReadInt32();
				reader.ReadUInt32();
				reader.ReadUInt32();
			}

			var ocaInput = (int)BinaryPrimitives.ReverseEndianness(reader.ReadUInt32());
			var ocaSync = (int)BinaryPrimitives.ReverseEndianness(reader.ReadUInt32());
			var roomLoad = (int)BinaryPrimitives.ReverseEndianness(reader.ReadUInt32());

			for (var a = 0; a < ocaInput; a++)
			{
				reader.ReadBytes(8);
			}

			for (var a = 0; a < ocaSync; a++)
			{
				reader.ReadBytes(8);
			}

			for (var a = 0; a < roomLoad; a++)
			{
				reader.ReadBytes(4);
			}

			result.RerecordCount = (int)BinaryPrimitives.ReverseEndianness(reader.ReadUInt32());

			// reading movie_last_recorded_frame for frame count
			result.Frames = (int)BinaryPrimitives.ReverseEndianness(reader.ReadUInt32());
		}
		catch (EndOfStreamException)
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
