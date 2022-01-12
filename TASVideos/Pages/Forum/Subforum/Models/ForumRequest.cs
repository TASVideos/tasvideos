using TASVideos.Core;

namespace TASVideos.Pages.Forum.Subforum.Models
{
	public class ForumRequest : PagingModel
	{
		public ForumRequest()
		{
			PageSize = ForumConstants.TopicsPerForum;
			Sort = $"-{nameof(ForumDisplayModel.ForumTopicEntry.Type)},-{nameof(ForumDisplayModel.ForumTopicEntry.LastPostDateTime)}";
		}
	}
}
