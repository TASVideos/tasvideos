using TASVideos.Data.Entity.Forum;

namespace TASVideos.Pages.Forum.Posts.Models
{
	public class ForumPostCreateModel
	{
		public bool IsLocked { get; set; }
		public string? UserAvatar { get; set; }
		public string? UserSignature { get; set; }

		public string TopicTitle { get; set; } = "";
		public string? Subject { get; set; }
		public string Text { get; set; } = "";
		public ForumPostMood Mood { get; set; } = ForumPostMood.Normal;
	}
}
