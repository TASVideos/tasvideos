using TASVideos.Data;

namespace TASVideos.Pages.Forum.Topics.Models
{
	public class TopicRequest : PagingModel
	{
		public TopicRequest()
		{
			PageSize = ForumConstants.PostsPerPage;
			Sort = $"{nameof(ForumTopicModel.ForumPostEntry.CreateTimestamp)}";
		}

		public int? Highlight { get; set; }
	}
}
