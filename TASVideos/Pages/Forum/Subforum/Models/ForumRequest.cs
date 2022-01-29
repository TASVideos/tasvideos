using TASVideos.Core;

namespace TASVideos.Pages.Forum.Subforum.Models;

public class ForumRequest : PagingModel
{
	public ForumRequest()
	{
		PageSize = ForumConstants.TopicsPerForum;
	}
}
