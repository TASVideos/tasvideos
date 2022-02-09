namespace TASVideos.Pages.RssFeeds.Models;

public class RssNews
{
	public int PostId { get; init; }
	public DateTime PubDate { get; set; }
	public string Subject { get; init; } = "";
	public string Text { get; init; } = "";
	public bool EnableHtml { get; set; }
	public bool EnableBbCode { get; set; }
}
