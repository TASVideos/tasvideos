using TASVideos.Core;

namespace TASVideos.RazorPages.Pages.Forum.Subforum.Models
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
