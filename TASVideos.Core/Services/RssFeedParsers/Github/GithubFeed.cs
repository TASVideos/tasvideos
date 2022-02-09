using System.Text.RegularExpressions;
using System.Xml.Serialization;

namespace TASVideos.Core.Services.RssFeedParsers.Github;

internal static class GithubFeed
{
	public static IEnumerable<CommitEntry> Parse(string xml, int max)
	{
		using var textReader = new StringReader(xml);
		var serializer = new XmlSerializer(typeof(GithubFeedResult));
		var result = (GithubFeedResult)serializer.Deserialize(textReader)!;
		return result.Entry
			.Select(e => new CommitEntry(
				e.Author?.Name ?? "",
				e.Updated ?? "",
				Regex.Replace(e.Content?.Text ?? "", "<.*?>", ""),
				e.Link?.Href?.ToString() ?? ""))
			.Take(max);
	}
}
