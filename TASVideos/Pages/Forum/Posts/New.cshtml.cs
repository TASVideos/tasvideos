using System;
using System.Threading.Tasks;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using TASVideos.Data;
using TASVideos.Data.Entity;
using TASVideos.Models;
using TASVideos.Tasks;

namespace TASVideos.Pages.Forum.Posts
{
	[Authorize]
	public class NewModel : BasePageModel
	{
		private readonly UserManager<User> _userManager;
		private readonly ForumTasks _forumTasks;

		public NewModel(
			UserManager<User> userManager,
			ForumTasks forumTasks,
			UserTasks userTasks)
		: base(userTasks)
		{
			_userManager = userManager;
			_forumTasks = forumTasks;
		}

		[FromQuery]
		public PagedModel Search { get; set; }

		public PageOf<PostsSinceLastVisitModel> Posts { get; set; }

		public async Task OnGet()
		{
			var user = await _userManager.GetUserAsync(User);
			Posts = await _forumTasks.GetPostsSinceLastVisit(
				Search, 
				user.LastLoggedInTimeStamp ?? DateTime.UtcNow,
				UserHas(PermissionTo.SeeRestrictedForums));

			foreach (var post in Posts)
			{
				post.RenderedText = RenderPost(post.Text, post.EnableBbCode, post.EnableBbCode);
				post.RenderedSignature = RenderSignature(post.Signature);
			}
		}
	}
}
