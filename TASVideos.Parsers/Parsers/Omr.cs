using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using System.Xml.XPath;

using SharpCompress.Readers;

using TASVideos.MovieParsers.Result;

namespace TASVideos.MovieParsers.Parsers
{
	internal class Omr : ParserBase, IParser
	{
		public override string FileExtension => "omr";

		public IParseResult Parse(Stream file)
		{
			var result = new ParseResult
			{
				Region = RegionType.Ntsc,
				FileExtension = FileExtension,
				SystemCode = SystemCodes.Msx
			};

			using (var reader = ReaderFactory.Open(file))
			{
				reader.MoveToNextEntry();
				using (var entry = reader.OpenEntryStream())
				using (var textReader = new StreamReader(entry))
				{
					var serial = XElement.Parse(textReader.ReadToEnd());
					var replay = serial.Descendants().First(x => x.Name == "replay");
					result.RerecordCount = int.Parse(replay.Descendants().First(x => x.Name == "reRecordCount").Value);

					var startsFromSavestate = replay
						.Descendants().First(x => x.Name == "snapshots")
						.Descendants().First(x => x.Name == "item" && x.Attributes().Any(a => a.Name == "id" && a.Value == "1"))
						.Descendants().First(x => x.Name == "scheduler")
						.Descendants().First(x => x.Name == "currentTime")
						.Descendants().First(x => x.Name == "time")
						.Value != "0";

					if (startsFromSavestate)
					{
						result.StartType = MovieStartType.Savestate;
					}

					var isPal = ((IEnumerable)replay.XPathEvaluate("//snapshots/item/config/device/palTiming"))
						.Cast<XElement>()
						.Any(x => x.Value.ToString() == "true");

					if (isPal)
					{
						result.Region = RegionType.Pal;
					}
				}
			}

			return result;
		}
	}
}
