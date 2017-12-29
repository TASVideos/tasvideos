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
		public IActionResult Submit(SubmissionCreateViewModel model)
		{
			return View(model);
		}
	}
}
