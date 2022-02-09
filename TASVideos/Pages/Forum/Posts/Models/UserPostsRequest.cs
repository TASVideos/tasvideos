using TASVideos.Core;

namespace TASVideos.Pages.Forum.Posts.Models;

public class UserPostsRequest : PagingModel
{
	public UserPostsRequest()
	{
		PageSize = ForumConstants.PostsPerPage;
		Sort = $"-{nameof(UserPostsModel.Post.CreateTimestamp)}";
	}
}
