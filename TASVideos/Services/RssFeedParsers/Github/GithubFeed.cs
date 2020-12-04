using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Xml.Serialization;

namespace TASVideos.Services.RssFeedParsers.Github
{
	public static class GithubFeed
	{
		public static IEnumerable<ICommitEntry> Parse(string xml, int max)
		{
			using var textReader = new StringReader(xml);
			var serializer = new XmlSerializer(typeof(GithubFeedResult));
			var result = (GithubFeedResult)serializer.Deserialize(textReader)!;
			return result.Entry
				.Select(e => new CommitEntry
				{
					Author = e.Author?.Name ?? "",
					At = e.Updated ?? "",
					Message = Regex.Replace( e.Content?.Text ?? "", "<.*?>", ""),
					Link = e.Link?.Href?.ToString() ?? ""
				})
				.Take(max);
		}
	}
}
