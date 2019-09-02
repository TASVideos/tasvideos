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
	[FileExtension("omr")]
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
					var replay = XElement.Parse(textReader.ReadToEnd())
						.Descendants().First(x => x.Name == "replay");

					result.RerecordCount = int.Parse(replay.Descendants().First(x => x.Name == "reRecordCount").Value);

					var isPowerOn = ((IEnumerable)replay.XPathEvaluate("//snapshots/item/scheduler/currentTime/time"))
						.Cast<XElement>()
						.Any(x => x.Value == "0");

					if (!isPowerOn)
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

					var lengthTimestamp = long.Parse(((IEnumerable)replay.XPathEvaluate("//events/item"))
						.Cast<XElement>()
						.Last(x => x.Attribute("type")?.Value != "EndLog")
						.Descendants()
						.First(x => x.Name == "StateChange")
						.Descendants()
						.First(x => x.Name == "time")
						.Descendants()
						.First(x => x.Name == "time")
						.Value);

					var seconds = ConvertTimestamp(lengthTimestamp);

					result.Frames = (int)Math.Ceiling(seconds * (result.Region == RegionType.Pal ? 50.1589758045661 : 59.9227510135505));
				}
			}

			return result;
		}

		private double ConvertTimestamp(long timestamp)
		{
			return timestamp / 3579545.0 / 960.0;
		}
	}
}
