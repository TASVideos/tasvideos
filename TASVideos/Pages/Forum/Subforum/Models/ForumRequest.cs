using TASVideos.Data;
using TASVideos.Data.Constants;

namespace TASVideos.Pages.Forum.Subforum.Models
{
	public class ForumRequest : PagedModel
	{
		public ForumRequest()
		{
			PageSize = ForumConstants.TopicsPerForum;
			Sort = $"-{nameof(ForumDisplayModel.ForumTopicEntry.CreateTimestamp)}";
		}
	}

}
