using System;
using System.IO;

using TASVideos.MovieParsers.Extensions;
using TASVideos.MovieParsers.Result;

namespace TASVideos.MovieParsers.Parsers
{
	[FileExtension("gmv")]
	internal class Gmv : ParserBase, IParser
	{
		public override string FileExtension => "gmv";

		public IParseResult Parse(Stream file)
		{
			var result = new ParseResult
			{
				Region = RegionType.Ntsc,
				FileExtension = FileExtension,
				SystemCode = SystemCodes.Genesis
			};

			using (var br = new BinaryReader(file))
			{
				var header = new string(br.ReadChars(16));
				if (!header.StartsWith("Gens Movie"))
				{
					return new ErrorResult("Invalid file format, does not seem to be a .gmv");
				}

				result.RerecordCount = br.ReadInt32();
				var controller1Config = br.ReadByte();
				var controller2Config = br.ReadByte();
				var flags = br.ReadByte();

				if (flags.Bit(7))
				{
					result.Region = RegionType.Pal;
				}

				br.ReadBytes(42); // movie name
			}

			return result;
		}
	}
}
