using TASVideos.Pages.Forum.Topics.Models;

namespace TASVideos.Pages.Forum.Posts.Models
{
	public class PostsSinceLastVisitModel : ForumTopicModel.ForumPostEntry
	{
		public string TopicTitle { get; set; }
		public int ForumId { get; set; }
		public string ForumName { get; set; }
	}
}
