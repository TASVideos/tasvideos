using System.Collections;
using System.Xml.Linq;
using System.Xml.XPath;

namespace TASVideos.MovieParsers.Parsers;

[FileExtension("omr")]
internal class Omr : ParserBase, IParser
{
	public override string FileExtension => "omr";

	public async Task<IParseResult> Parse(Stream file, long length)
	{
		var result = new ParseResult
		{
			Region = RegionType.Ntsc,
			FileExtension = FileExtension,
			SystemCode = SystemCodes.Msx
		};

		await using var gz = new GZipStream(file, CompressionMode.Decompress);
		using var unzip = new StreamReader(gz);
		var replay = XElement.Parse(await unzip.ReadToEndAsync())
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

		result.Frames = (int)Math.Round(seconds * (result.Region == RegionType.Pal ? 50.1589758045661 : 59.9227510135505));

		var version = ((IEnumerable)replay.XPathEvaluate("//snapshots/item"))
			.Cast<XElement>()
			.Attributes()
			.First(x => x.Name == "version")
			.Value;

		string system;

		if(Convert.ToInt16(version) >= 4)
		{
			var confVersion = ((IEnumerable)replay.XPathEvaluate("//snapshots/item/config"))
				.Cast<XElement>()
				.Attributes()
				.First(x => x.Name == "version")
				.Value;

			if(Convert.ToInt16(confVersion) >= 6)
			{
				system = ((IEnumerable)replay.XPathEvaluate("//snapshots/item/config/config/msxconfig/info"))
					.Cast<XElement>()
					.Descendants()
					.First(x => x.Name == "type")
					.Value
					.ToLower();
			}
			else
			{
				system = ((IEnumerable)replay.XPathEvaluate("//snapshots/item/config/config/children/item/children/item"))
					.Cast<XElement>()
					.First(x => x.Descendants().Any(d => d.Name == "name" && d.Value == "type"))
					.Descendants()
					.First(x => x.Name == "data")
					.Value
					.ToLower();
			}
		}
		else
		{
			system = ((IEnumerable)replay.XPathEvaluate("//snapshots/item/config/config/children/item/children/item"))
				.Cast<XElement>()
				.First(x => x.Descendants().Any(d => d.Name == "name" && d.Value == "type"))
				.Descendants()
				.First(x => x.Name == "data")
				.Value
				.ToLower();
		}

		if (system.StartsWith("svi"))
		{
			result.SystemCode = SystemCodes.Svi;
		}
		else if (system.StartsWith("coleco"))
		{
			result.SystemCode = SystemCodes.Coleco;
		}
		else if (system.StartsWith("sg-1000"))
		{
			result.SystemCode = SystemCodes.Sg;
		}

		return result;
	}

	private static double ConvertTimestamp(long timestamp) => timestamp / 3579545.0 / 960.0;
}
