using TASVideos.Data.Entity;

namespace TASVideos.Pages.Forum.Posts.Models;

public class MiniPostModel
{
	public DateTime CreateTimestamp { get; set; }
	public string PosterName { get; set; } = "";
	public PreferredPronounTypes PosterPronouns { get; set; }
	public string Text { get; set; } = "";
	public bool EnableHtml { get; set; }
	public bool EnableBbCode { get; set; }
}
