using TASVideos.Data;
using TASVideos.Data.Constants;

namespace TASVideos.Pages.Forum.Posts.Models
{
	public class UserPostsRequest : PagedModel
	{
		public UserPostsRequest()
		{
			PageSize = ForumConstants.PostsPerPage;
			Sort = $"-{nameof(UserPostsModel.Post.CreateTimestamp)}";
		}
	}
}
