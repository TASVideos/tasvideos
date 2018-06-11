using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

using TASVideos.Data;
using TASVideos.Data.Entity;
using TASVideos.Filter;
using TASVideos.Models;
using TASVideos.Tasks;

namespace TASVideos.Controllers
{
	public class ForumController : BaseController
	{
		private readonly ForumTasks _forumTasks;
		private readonly UserManager<User> _userManager;

		public ForumController(
			ForumTasks forumTasks,
			UserTasks userTasks,
			UserManager<User> userManager)
			: base(userTasks)
		{
			_forumTasks = forumTasks;
			_userManager = userManager;
		}

		[AllowAnonymous]
		public async Task<IActionResult> Index()
		{
			var model = await _forumTasks.GetForumIndex();
			foreach (var m in model.Categories)
			{
				m.Description = RenderHtml(m.Description);
				foreach (var f in m.Forums)
				{
					f.Description = RenderHtml(f.Description);
				}
			}

			return View(model);
		}

		[AllowAnonymous]
		public async Task<IActionResult> Subforum(ForumRequest request)
		{
			var model = await _forumTasks.GetForumForDisplay(request);

			if (model != null)
			{
				model.Description = RenderHtml(model.Description);
				return View(model);
			}

			return NotFound();
		}

		[AllowAnonymous]
		public async Task<IActionResult> Topic(TopicRequest request)
		{
			var model = await _forumTasks.GetTopicForDisplay(request);

			if (model != null)
			{
				foreach (var post in model.Posts)
				{
					post.RenderedText = RenderPost(post.Text, post.EnableBbCode, post.EnableHtml);
					post.RenderedSignature = RenderPost(post.Signature, true, false); // BBcode on, Html off hardcoded, do we want this to be configurable?
				}

				if (model.Poll != null)
				{
					model.Poll.Question = RenderPost(model.Poll.Question, false, true); // TODO: do we have bbcode in poll questions??
				}

				return View(model);
			}

			return NotFound();
		}

		[AllowAnonymous]
		public async Task<IActionResult> UnansweredPosts(PagedModel paging)
		{
			var model = await _forumTasks.GetUnansweredPosts(paging);
			return View(model);
		}

		[HttpPost, ValidateAntiForgeryToken]
		[RequirePermission(PermissionTo.VoteInPolls)]
		public async Task<IActionResult> Vote(int pollId, int ordinal)
		{
			var user = await _userManager.GetUserAsync(User);
			var topicId = await _forumTasks.Vote(user, pollId, ordinal, IpAddress.ToString());

			if (topicId == null)
			{
				return BadRequest();
			}

			return RedirectToAction(nameof(Topic), new { Id = topicId });
		}

		[RequirePermission(PermissionTo.SeePollResults)]
		public async Task<IActionResult> ViewPollResults(int id)
		{
			var model = await _forumTasks.GetPollResults(id);

			if (model == null)
			{
				return NotFound();
			}

			model.Question = RenderPost(model.Question, true, false); // TODO: flags

			return View(model);
		}

		// TODO: auto-add topic permission based on post count, also ability to vote
		[Authorize]
		[RequirePermission(PermissionTo.CreateForumTopics)]
		public async Task<IActionResult> CreateTopic(int forumId)
		{
			var model = await _forumTasks.GetTopicCreateData(forumId);

			if (model == null)
			{
				return NotFound();
			}

			return View(model);
		}

		[HttpPost, ValidateAntiForgeryToken]
		[RequirePermission(PermissionTo.CreateForumTopics)]
		public async Task<IActionResult> CreateTopic(TopicCreatePostModel model)
		{
			if (!ModelState.IsValid)
			{
				return View(model);
			}

			var user = await _userManager.GetUserAsync(User);
			var topicId = await _forumTasks.CreateTopic(model, user, IpAddress.ToString());

			return RedirectToAction(nameof(Topic), "Forum", new { Id = topicId });
		}

		[Authorize]
		[RequirePermission(PermissionTo.CreateForumPosts)]
		public async Task<IActionResult> CreatePost(int topicId, int? quoteId = null)
		{
			var user = await _userManager.GetUserAsync(User);
			var model = await _forumTasks.GetCreatePostData(topicId, quoteId);

			if (model == null)
			{
				return NotFound();
			}

			if (model.IsLocked && !UserPermissions.Contains(PermissionTo.PostInLockedTopics))
			{
				return AccessDenied();
			}

			return View(model);
		}

		[RequirePermission(PermissionTo.CreateForumPosts)]
		[HttpPost, ValidateAntiForgeryToken]
		public async Task<IActionResult> CreatePost(ForumPostModel model)
		{
			if (!ModelState.IsValid)
			{
				return View(model);
			}

			if (!UserPermissions.Contains(PermissionTo.PostInLockedTopics)
				&& await _forumTasks.IsTopicLocked(model.TopicId))
			{
				return AccessDenied();
			}

			var user = await _userManager.GetUserAsync(User);
			await _forumTasks.CreatePost(model, user, IpAddress.ToString());

			return RedirectToAction(nameof(Topic), "Forum", new { id = model.TopicId });
		}

		[Authorize]
		public async Task<IActionResult> EditPost(int id)
		{
			var model = await _forumTasks.GetEditPostData(id);
			if (model == null)
			{
				return NotFound();
			}

			if (!UserPermissions.Contains(PermissionTo.EditForumPosts) && !model.IsLastPost)
			{
				return AccessDenied();
			}

			model.RenderedText = RenderPost(model.Text, model.EnableBbCode, model.EnableHtml);

			// TODO: check if author and last post, or permission to edit posts
			return View(model);
		}

		[Authorize]
		[HttpPost, ValidateAntiForgeryToken]
		public async Task<IActionResult> EditPost(ForumPostEditModel model)
		{
			if (!ModelState.IsValid)
			{
				model.RenderedText = RenderPost(model.Text, model.EnableBbCode, model.EnableHtml);
				return View(model);
			}

			if (!UserPermissions.Contains(PermissionTo.EditForumPosts))
			{
				// check is last post (could have changed)
				// return view and modelstate error
			}

			await _forumTasks.EditPost(model);

			// TODO: check if author and last post, or permission to edit posts
			return RedirectToAction(nameof(Topic), "Forum", new { id = model.TopicId });
		}

		[HttpPost]
		[RequirePermission(PermissionTo.CreateForumPosts)]
		public IActionResult GeneratePreview()
		{
			var text = new StreamReader(Request.Body, Encoding.UTF8).ReadToEnd();
			var renderedText = RenderPost(text, true, false); // TODO: pass in bbcode flag

			return new ContentResult { Content = renderedText };
		}
	}
}
