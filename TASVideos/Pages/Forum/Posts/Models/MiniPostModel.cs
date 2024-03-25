using TASVideos.Data.Entity;

namespace TASVideos.Pages.Forum.Posts.Models;

public class MiniPostModel
{
	public DateTime CreateTimestamp { get; init; }
	public string PosterName { get; init; } = "";
	public PreferredPronounTypes PosterPronouns { get; init; }
	public string Text { get; init; } = "";
	public bool EnableHtml { get; init; }
	public bool EnableBbCode { get; init; }
}
