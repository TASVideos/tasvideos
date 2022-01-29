using TASVideos.Core;
using TASVideos.Pages.Forum.Posts.Models;

namespace TASVideos.Pages.Forum.Topics.Models;

public class TopicRequest : PagingModel
{
	public TopicRequest()
	{
		PageSize = ForumConstants.PostsPerPage;
		Sort = $"{nameof(ForumPostEntry.CreateTimestamp)}";
	}

	public int? Highlight { get; set; }
}
