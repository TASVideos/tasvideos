using System.Threading.Tasks;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

using TASVideos.Data.Entity;
using TASVideos.Models;
using TASVideos.Services.ExternalMediaPublisher;
using TASVideos.Tasks;

namespace TASVideos.Pages.Forum.Posts
{
	[Authorize]
	public class EditModel : BasePageModel
	{
		private readonly ForumTasks _forumTasks;
		private readonly UserManager<User> _userManager;
		private readonly ExternalMediaPublisher _publisher;

		public EditModel(
			UserManager<User> userManager,
			ExternalMediaPublisher publisher,
			ForumTasks forumTasks,
			UserTasks userTasks)
			: base(userTasks)
		{
			_userManager = userManager;
			_publisher = publisher;
			_forumTasks = forumTasks;
		}

		[FromRoute]
		public int Id { get; set; }

		[BindProperty]
		public ForumPostEditModel Post { get; set; }

		public async Task<IActionResult> OnGet()
		{
			Post = await _forumTasks.GetEditPostData(Id, UserHas(PermissionTo.SeeRestrictedForums));
			if (Post == null)
			{
				return NotFound();
			}

			var userId = int.Parse(_userManager.GetUserId(User));

			if (!UserHas(PermissionTo.EditForumPosts)
				&& !(Post.IsLastPost && Post.PosterId == userId))
			{
				return AccessDenied();
			}

			Post.RenderedText = RenderPost(Post.Text, Post.EnableBbCode, Post.EnableHtml);

			return Page();
		}

		public async Task<IActionResult> OnPost()
		{
			if (!ModelState.IsValid)
			{
				Post.RenderedText = RenderPost(Post.Text, Post.EnableBbCode, Post.EnableHtml);
				return Page();
			}

			if (!UserHas(PermissionTo.EditForumPosts))
			{
				var userId = int.Parse(_userManager.GetUserId(User));
				if (!(await _forumTasks.CanEdit(Id, userId)))
				{
					ModelState.AddModelError("", "Unable to edit post. It is no longer the latest post.");
					return Page();
				}
			}

			var topic = await _forumTasks.GetTopic(Post.TopicId);
			if (topic == null
				|| (topic.Forum.Restricted && !UserHas(PermissionTo.SeeRestrictedForums)))
			{
				return NotFound();
			}

			_publisher.SendForum(
				topic.Forum.Restricted,
				$"Post edited by {User.Identity.Name} ({topic.Forum.ShortName}: {topic.Title})",
				"",
				$"{BaseUrl}/p/{Id}#{Id}");

			await _forumTasks.EditPost(Id, Post);

			return RedirectToPage("/Forum/Topic", new { Id = Post.TopicId });
		}

		public async Task<IActionResult> OnGetDelete()
		{
			var result = await _forumTasks.DeletePost(
				Id,
				UserHas(PermissionTo.DeleteForumPosts),
				UserHas(PermissionTo.SeeRestrictedForums));
			if (result == null)
			{
				return NotFound();
			}

			var topic = await _forumTasks.GetTopic(result.Value);
			_publisher.SendForum(
				topic.Forum.Restricted,
				$"Post DELETED by {User.Identity.Name} ({topic.Forum.ShortName}: {topic.Title})",
				$"{BaseUrl}/p/{Id}#{Id}",
				$"{BaseUrl}/Forum/Topic/{topic.Id}");

			return RedirectToPage("/Forum/Topics", new { id = result });
		}
	}
}
