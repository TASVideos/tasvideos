using TASVideos.Data.Entity;

namespace TASVideos.Pages.RssFeeds.Models;

public class RssSubmission
{
	public int Id { get; init; }
	public int? TopicId { get; init; }
	public DateTime CreateTimestamp { get; init; }
	public string Title { get; init; } = "";
	public WikiPage Wiki { get; init; } = new();
}
