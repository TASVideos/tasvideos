namespace TASVideos.Pages.RssFeeds.Models;

public class RssWiki
{
	public string RevisionMessage { get; init; } = "";
	public string PageName { get; init; } = "";
	public DateTime PubDate { get; init; }
	public bool IsNew { get; init; }
	public string Author { get; init; } = "";
}
