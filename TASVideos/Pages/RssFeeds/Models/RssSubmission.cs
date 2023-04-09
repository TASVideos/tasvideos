using TASVideos.Core.Services.Wiki;

namespace TASVideos.Pages.RssFeeds.Models;

public class RssSubmission
{
	public int Id { get; init; }
	public int? TopicId { get; init; }
	public DateTime CreateTimestamp { get; init; }
	public string Title { get; init; } = "";
	public IWikiPage Wiki { get; set; } = null!;
}
