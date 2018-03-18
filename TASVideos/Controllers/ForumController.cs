using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

using TASVideos.Tasks;

namespace TASVideos.Controllers
{
	public class ForumController : BaseController
	{
		private readonly ForumTasks _forumTasks;

		public ForumController(
			ForumTasks forumTasks,
			UserTasks userTasks)
			: base(userTasks)
		{
			_forumTasks = forumTasks;
		}

		[AllowAnonymous]
		public async Task<IActionResult> Index()
		{
			var model = await _forumTasks.GetForumIndex();
			return View(model);
		}
	}
}
