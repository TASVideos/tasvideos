using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using TASVideos.Tasks;

namespace TASVideos.Controllers
{
    public class GameController : BaseController
    {
		public GameController(UserTasks userTasks)
			: base(userTasks)
		{
		}

		public IActionResult Index(int id)
		{
			return View();
		}
	}
}
