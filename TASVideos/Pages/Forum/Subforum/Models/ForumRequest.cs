using TASVideos.Data;

namespace TASVideos.Pages.Forum.Subforum.Models
{
	public class ForumRequest : PagedModel
	{
		public ForumRequest()
		{
			PageSize = 50;
			SortDescending = true;
			SortBy = nameof(ForumDisplayModel.ForumTopicEntry.CreateTimestamp);
		}
	}

}
