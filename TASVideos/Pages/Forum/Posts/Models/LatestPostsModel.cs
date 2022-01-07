namespace TASVideos.Pages.Forum.Posts.Models
{
	public class LatestPostsModel : ForumPostEntry
	{
		public string TopicTitle { get; set; } = "";
		public int ForumId { get; set; }
		public string ForumName { get; set; } = "";
	}
}
