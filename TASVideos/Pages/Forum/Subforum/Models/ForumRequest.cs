using TASVideos.Data;

namespace TASVideos.Pages.Forum.Subforum.Models
{
	public class ForumRequest : PagingModel
	{
		public ForumRequest()
		{
			PageSize = ForumConstants.TopicsPerForum;
			Sort = $"-{nameof(ForumDisplayModel.ForumTopicEntry.CreateTimestamp)}";
		}
	}

}
