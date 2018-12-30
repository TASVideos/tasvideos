using System.Threading.Tasks;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

using TASVideos.Data.Entity;
using TASVideos.Tasks;

namespace TASVideos.Controllers
{
	public class UserController : BaseController
	{
		private readonly AwardTasks _awardTasks;

		public UserController(
			UserTasks userTasks,
			AwardTasks awardTasks)
			: base(userTasks)
		{
			_awardTasks = awardTasks;
		}

		[AllowAnonymous]
		[Route("[controller]/[action]/{userName}")]
		public async Task<IActionResult> Ratings(string userName)
		{
			var model = await UserTasks.GetUserRatings(userName, UserHas(PermissionTo.SeePrivateRatings));
			if (model == null)
			{
				return NotFound();
			}

			ViewData["IsMyPage"] = false;
			return View(model);
		}
	}
}
