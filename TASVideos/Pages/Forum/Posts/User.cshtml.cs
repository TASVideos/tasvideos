using System.Threading.Tasks;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

using TASVideos.Data.Entity;
using TASVideos.Models;
using TASVideos.Tasks;

namespace TASVideos.Pages.Forum.Posts
{
	[AllowAnonymous]
	public class UserModel : BasePageModel
	{
		private readonly ForumTasks _forumTasks;

		public UserModel(
			ForumTasks forumTasks,
			UserTasks userTasks)
			: base(userTasks)
		{
			_forumTasks = forumTasks;
		}

		[FromRoute]
		public string UserName { get; set; }

		[FromQuery]
		public UserPostsRequest Search { get; set; }

		public UserPostsModel UserPosts { get; set; }

		public async Task<IActionResult> OnGet()
		{
			UserPosts = await _forumTasks.PostsByUser(Search, UserHas(PermissionTo.SeeRestrictedForums));

			if (UserPosts == null)
			{
				return NotFound();
			}

			UserPosts.RenderedSignature = RenderSignature(UserPosts.Signature); 
			foreach (var post in UserPosts.Posts)
			{
				post.RenderedText = RenderPost(post.Text, post.EnableBbCode, post.EnableHtml);
			}

			return Page();
		}
	}
}
