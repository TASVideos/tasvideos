using System.Collections;
using System.Xml.Linq;
using System.Xml.XPath;

namespace TASVideos.MovieParsers.Parsers;

[FileExtension("omr")]
internal class Omr : Parser, IParser
{
	public async Task<IParseResult> Parse(Stream file, long length)
	{
		var result = new SuccessResult(FileExtension)
		{
			Region = RegionType.Ntsc,
			SystemCode = SystemCodes.Msx
		};

		await using var gz = new GZipStream(file, CompressionMode.Decompress);
		using var unzip = new StreamReader(gz);
		var replay = XElement.Parse(await unzip.ReadToEndAsync()).FirstDescendant("replay");

		result.RerecordCount = int.Parse(replay.FirstDescendant("reRecordCount").Value);

		var isPowerOn = replay.XPathEvaluateList("//snapshots/item/scheduler/currentTime/time")
			.Any(x => x.Value == "0");

		if (!isPowerOn)
		{
			result.StartType = MovieStartType.Savestate;
		}

		var isPal = replay.XPathEvaluateList("//snapshots/item/config/device/palTiming")
			.Any(x => x.Value == "true");

		if (isPal)
		{
			result.Region = RegionType.Pal;
		}

		var lengthTimestamp = long.Parse(replay.XPathEvaluateList("//events/item")
			.Last(x => x.Attribute("type")?.Value != "EndLog")
			.FirstDescendant("StateChange")
			.FirstDescendant("time")
			.FirstDescendant("time")
			.Value);

		var seconds = ConvertTimestamp(lengthTimestamp);

		result.Frames = (int)Math.Round(seconds * (result.Region == RegionType.Pal ? 50.1589758045661 : 59.9227510135505));

		var version = replay.XPathEvaluateList("//snapshots/item").FirstAttributeValue("version");

		string system;

		if (Convert.ToInt16(version) >= 4)
		{
			var confVersion = replay.XPathEvaluateList("//snapshots/item/config").FirstAttributeValue("version");

			if (Convert.ToInt16(confVersion) >= 6)
			{
				system = replay.XPathEvaluateList("//snapshots/item/config/config/msxconfig/info")
					.FirstDescendant("type").Value.ToLower();
			}
			else
			{
				system = LegacyGetSystem(replay);
			}
		}
		else
		{
			system = LegacyGetSystem(replay);
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

	private static string LegacyGetSystem(XElement replay)
		=> replay.XPathEvaluateList("//snapshots/item/config/config/children/item/children/item")
				.First(x => x.Descendants().Any(d => d.Name == "name" && d.Value == "type"))
				.FirstDescendant("data")
				.Value.ToLower();

	private static double ConvertTimestamp(long timestamp) => timestamp / 3579545.0 / 960.0;
}

internal static class XPathExtensions
{
	public static IEnumerable<XElement> XPathEvaluateList(this XElement element, string path)
		=> ((IEnumerable)element.XPathEvaluate(path)).Cast<XElement>();

	public static XElement FirstDescendant(this XElement element, string name)
		=> element.Descendants().First(x => x.Name == name);

	public static XElement FirstDescendant(this IEnumerable<XElement> element, string name)
		=> element.Descendants().First(x => x.Name == name);

	public static string FirstAttributeValue(this IEnumerable<XElement> element, string name)
		=> element.Attributes().First(x => x.Name == name).Value;
}
