using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
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

		public ForumController(
			UserTasks userTasks,
			ForumTasks forumTasks)
			: base(userTasks)
		{
			_forumTasks = forumTasks;
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
	}
}
