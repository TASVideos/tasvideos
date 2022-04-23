using TASVideos.Core;

namespace TASVideos.Pages.Forum.Posts.Models;

public class LatestPostsModel
{
	public int Id { get; set; }
	public int TopicId { get; set; }
	public string TopicTitle { get; set; } = "";
	public int ForumId { get; set; }
	public string ForumName { get; set; } = "";

	[Sortable]
	public DateTime CreateTimestamp { get; set; }

	public string Text { get; set; } = "";

	public string PosterName { get; set; } = "";
}
