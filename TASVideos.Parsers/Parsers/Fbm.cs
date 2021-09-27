using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TASVideos.MovieParsers.Result;

namespace TASVideos.MovieParsers.Parsers
{
	[FileExtension("fbm")]
	internal class Fbm : ParserBase, IParser
	{
		public override string FileExtension => "fbm";

		public async Task<IParseResult> Parse(Stream file)
		{
			var result = new ParseResult
			{
				Region = RegionType.Ntsc,
				FileExtension = FileExtension,
				SystemCode = SystemCodes.Arcade
			};

			using var br = new BinaryReader(file);
			var header = new string(br.ReadChars(4));
			if (header != "FB1 ")
			{
				return new ErrorResult("Invalid file format, does not seem to be a .fbm");
			}

			br.ReadByte(); // Version number
			var nextHeader = new string(br.ReadChars(4));
			if (nextHeader == "FS1 ")
			{
				return new ErrorResult("Savestate movies not supported yet!");
			}

			if (nextHeader != "FR1 ")
			{
				return new ErrorResult("Input data not found");
			}

			br.ReadBytes(4); // Size of frame data chunk in bytes (not including the chunk identifier)
			result.Frames = br.ReadInt32();
			result.RerecordCount = br.ReadInt32();

			return await Task.FromResult(result);
		}
	}
}
