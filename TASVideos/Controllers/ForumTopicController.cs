using System.IO;
using System.Text;
using System.Threading.Tasks;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

using TASVideos.Data.Entity;
using TASVideos.Filter;
using TASVideos.Models;
using TASVideos.Tasks;


namespace TASVideos.Controllers
{
	public class ForumTopicController : BaseController
	{
		private readonly ForumTasks _forumTasks;
		private readonly UserManager<User> _userManager;

		public ForumTopicController(
			ForumTasks forumTasks,
			UserTasks userTasks,
			UserManager<User> userManager)
			: base(userTasks)
		{
			_forumTasks = forumTasks;
			_userManager = userManager;
		}

		[AllowAnonymous]
		public async Task<IActionResult> Index(TopicRequest request)
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

		// TODO: permissions, maybe a permission that is auto-added based on post count?
		[Authorize]
		public async Task<IActionResult> Create(int forumId)
		{
			var model = await _forumTasks.GetTopicCreateData(forumId);

			if (model == null)
			{
				return NotFound();
			}

			return View(model);
		}

		[Authorize]
		[HttpPost, ValidateAntiForgeryToken]
		public async Task<IActionResult> Create(TopicCreatePostModel model)
		{
			if (!ModelState.IsValid)
			{
				return View(model);
			}

			var user = await _userManager.GetUserAsync(User);
			var topicId = await _forumTasks.CreateTopic(model, user, IpAddress.ToString());

			return RedirectToAction(nameof(Index), new { Id = topicId });
		}

		// TODO: permission
		[Authorize]
		[HttpPost]
		public IActionResult GeneratePreview(string post)
		{
			var text = new StreamReader(Request.Body, Encoding.UTF8).ReadToEnd();
			var renderedText = RenderPost(text, true, false); // TODO: pass in bbcode flag

			return new ContentResult { Content = renderedText };
		}

		[Authorize]
		[HttpPost, ValidateAntiForgeryToken]
		public async Task<IActionResult> Vote(int pollId, int ordinal)
		{
			var user = await _userManager.GetUserAsync(User);
			var topicId = await _forumTasks.Vote(user, pollId, ordinal, IpAddress.ToString());

			if (topicId == null)
			{
				return BadRequest();
			}

			return RedirectToAction(nameof(Index), new { Id = topicId });
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
