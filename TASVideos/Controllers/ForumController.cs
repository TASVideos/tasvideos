using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using Microsoft.AspNetCore.Mvc;

using TASVideos.Tasks;

namespace TASVideos.Controllers
{
	public class ForumController : BaseController
	{
		public ForumController(UserTasks userTasks)
			: base(userTasks)
		{
		}

		public IActionResult Index()
		{
			return View();
		}
	}
}
