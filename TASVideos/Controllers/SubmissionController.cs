using Microsoft.AspNetCore.Mvc;
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
		public IActionResult Index()
		{
			return View();
		}

		public IActionResult Submit(SubmissionCreateViewModel model)
		{
			return View(model);
		}
	}
}
