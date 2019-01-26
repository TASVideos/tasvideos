using System.Linq;
using System.Threading.Tasks;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

using TASVideos.Data;
using TASVideos.Data.Entity;
using TASVideos.Data.Entity.Forum;
using TASVideos.Models;
using TASVideos.Services.ExternalMediaPublisher;
using TASVideos.Tasks;

namespace TASVideos.Pages.Forum.Posts
{
	[Authorize]
	public class EditModel : BasePageModel
	{
		private readonly ApplicationDbContext _db;
		private readonly ForumTasks _forumTasks;
		private readonly ExternalMediaPublisher _publisher;

		public EditModel(
			ApplicationDbContext db,
			ExternalMediaPublisher publisher,
			ForumTasks forumTasks,
			UserTasks userTasks)
			: base(userTasks)
		{
			_db = db;
			_publisher = publisher;
			_forumTasks = forumTasks;
		}

		[FromRoute]
		public int Id { get; set; }

		[BindProperty]
		public ForumPostEditModel Post { get; set; }

		public async Task<IActionResult> OnGet()
		{
			var seeRestricted = UserHas(PermissionTo.SeeRestrictedForums);
			Post = await _db.ForumPosts
				.ExcludeRestricted(seeRestricted)
				.Where(p => p.Id == Id)
				.Select(p => new ForumPostEditModel
				{
					CreateTimestamp = p.CreateTimeStamp,
					PosterId = p.PosterId,
					PosterName = p.Poster.UserName,
					EnableBbCode = p.EnableBbCode,
					EnableHtml = p.EnableHtml,
					TopicId = p.TopicId ?? 0,
					TopicTitle = p.Topic.Title,
					Subject = p.Subject,
					Text = p.Text
				})
				.SingleOrDefaultAsync();

			if (Post == null)
			{
				return NotFound();
			}

			var lastPostId = (await _db.ForumPosts
				.ForTopic(Post.TopicId)
				.ByMostRecent()
				.FirstAsync())
				.Id;

			Post.IsLastPost = Id == lastPostId;

			if (!UserHas(PermissionTo.EditForumPosts)
				&& !(Post.IsLastPost && Post.PosterId == UserId))
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
				if (!await _forumTasks.CanEdit(Id, UserId))
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

			return RedirectToPage("/Forum/Topics/Index", new { Id = Post.TopicId });
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

			return RedirectToPage("/Forum/Topics/Index", new { id = result });
		}
	}
}
