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
	public class ForumController : BaseController
	{
		private readonly UserManager<User> _userManager;
		private readonly ForumTasks _forumTasks;

		public ForumController(
			UserTasks userTasks,
			UserManager<User> userManager,
			ForumTasks forumTasks)
			: base(userTasks)
		{
			_userManager = userManager;
			_forumTasks = forumTasks;
		}

		[AllowAnonymous]
		public async Task<IActionResult> Index()
		{
			var model = await _forumTasks.GetForumIndex();
			return View(model);
		}

		[AllowAnonymous]
		public async Task<IActionResult> Forum(ForumRequest request)
		{
			var model = await _forumTasks.GetForumForDisplay(request);

			if (model != null)
			{
				return View(model);
			}

			return NotFound();
		}

		[Authorize]
		public async Task<IActionResult> Inbox()
		{
			var user = await _userManager.GetUserAsync(User);
			var model = await _forumTasks.GetUserInBox(user);
			return View(model);
		}

		[Authorize]
		public async Task<IActionResult> PrivateMessage(int id)
		{
			var user = await _userManager.GetUserAsync(User);
			var model = await _forumTasks.GetPrivateMessage(user, id);

			if (model == null)
			{
				return NotFound();
			}

			return View(model);
		}
	}
}
