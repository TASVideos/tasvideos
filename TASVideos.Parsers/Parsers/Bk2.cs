using System;
using System.IO;
using System.IO.Compression;
using System.Linq;

namespace TASVideos.MovieParsers
{
	[FileExtension("bk2")]
	internal class Bk2 : IParser
	{
		public string FileExtension => "bk2";

		public IParseResult Parse(Stream file)
		{
			var bk2Archive = new ZipArchive(file);
			var inputLogEntry = bk2Archive.Entries.Single(e => e.Name == "Input Log.txt");
			
			return new ParseResult
			{
				Region = RegionType.Ntsc,
				Frames = new Random().Next(10000, 250000),
				SystemCode = "NES",
				RerecordCount = new Random().Next(10000, 50000)
			};
		}
	}
}
