using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using TASVideos.Models;
using TASVideos.Tasks;

namespace TASVideos.Controllers
{
	public class HomeController : BaseController
	{
		public HomeController(UserTasks userTasks)
			: base (userTasks)
		{
		}

		public IActionResult Index()
		{
			return View(UserPermissions);
		}

		public IActionResult Error()
		{
			return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
		}
	}
}
