using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TASVideos.Data.Entity;
using TASVideos.Filter;
using TASVideos.Models;
using TASVideos.Tasks;

namespace TASVideos.Controllers
{
    public class SubmissionController : BaseController
    {
		public SubmissionController(UserTasks userTasks)
			: base(userTasks)
		{
		}

		// Submisison List
		[AllowAnonymous]
		public IActionResult Index()
		{
			return View();
		}

		[RequirePermission(PermissionTo.SubmitMovies)]
		public IActionResult Submit()
		{
			var model = new SubmissionCreateViewModel();
			return View(model);
		}

		[HttpPost]
		public IActionResult Submit(SubmissionCreateViewModel model)
		{
			if (ModelState.IsValid)
			{
				// TODO: save data
				return RedirectToAction(nameof(Index)); // TODO: reroute to actual submission
			}

			return View(model);
		}
	}
}
