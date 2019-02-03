using TASVideos.Data;
using TASVideos.Data.Constants;

namespace TASVideos.Pages.Forum.Topics.Models
{
	public class TopicRequest : PagedModel
	{
		public TopicRequest()
		{
			PageSize = ForumConstants.PostsPerPage;
			SortDescending = false;
			SortBy = nameof(ForumTopicModel.ForumPostEntry.CreateTimestamp);
		}

		public int? Highlight { get; set; }
	}
}
