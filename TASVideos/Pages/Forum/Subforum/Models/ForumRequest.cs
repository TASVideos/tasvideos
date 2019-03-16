using TASVideos.Data;

namespace TASVideos.Pages.Forum.Subforum.Models
{
	public class ForumRequest : PagedModel
	{
		public ForumRequest()
		{
			PageSize = 50;
			Sort = $"-{nameof(ForumDisplayModel.ForumTopicEntry.CreateTimestamp)}";
		}
	}

}
