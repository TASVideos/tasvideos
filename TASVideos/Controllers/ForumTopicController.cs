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

		// TODO: auto-add topic permission based on post count, also ability to vote
		[Authorize]
		[RequirePermission(PermissionTo.CreateForumTopics)]
		public async Task<IActionResult> Create(int forumId)
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
		public async Task<IActionResult> Create(TopicCreatePostModel model)
		{
			if (!ModelState.IsValid)
			{
				return View(model);
			}

			var user = await _userManager.GetUserAsync(User);
			var topicId = await _forumTasks.CreateTopic(model, user, IpAddress.ToString());

			return RedirectToAction(nameof(ForumController.Topic), "Forum", new { Id = topicId });
		}

		// TODO: move this to ForumPostController
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
