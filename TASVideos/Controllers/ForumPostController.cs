using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

using TASVideos.Data.Entity;
using TASVideos.Models;
using TASVideos.Tasks;

namespace TASVideos.Controllers
{
    public class ForumPostController : BaseController
	{
		private readonly ForumTasks _forumTasks;
		private readonly UserManager<User> _userManager;

		public ForumPostController(
			ForumTasks forumTasks,
			UserTasks userTasks,
			UserManager<User> userManager)
			: base(userTasks)
		{
			_forumTasks = forumTasks;
			_userManager = userManager;
		}

		// TODO: permission, auto-added on register?
		[Authorize]
		public IActionResult Create(int topicId)
		{
			return View(new ForumPostModel { TopicId = topicId });
		}

		// TODO: permission
		[Authorize]
		[HttpPost]
		public async Task<IActionResult> Create(ForumPostModel model)
		{
			if (!ModelState.IsValid)
			{
				return View(model);
			}

			// TODO: check if topic can have new posts and user is allowed to post to it
			var user = await _userManager.GetUserAsync(User);
			await _forumTasks.CreatePost(model, user, IpAddress.ToString());

			return RedirectToAction(nameof(ForumTopicController.Index), "ForumTopic", new { id = model.TopicId });
		}
	}
}
