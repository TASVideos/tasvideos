using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TASVideos.Tasks;

namespace TASVideos.Controllers
{
	public class RatingsController : BaseController
	{
		private readonly RatingsTasks _ratingsTasks;

		public RatingsController(
			RatingsTasks ratingsTasks,
			UserTasks userTasks)
			: base(userTasks)
		{
			_ratingsTasks = ratingsTasks;
		}

		[AllowAnonymous]
		public async Task<IActionResult> ViewPublication(int id)
		{
			var model = await _ratingsTasks.GetRatingsForPublication(id);
			if (model == null)
			{
				return NotFound();
			}

			return View(model);
		}
	}
}
