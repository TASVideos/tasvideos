using System.Collections.Generic;
using System.Threading.Tasks;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

using TASVideos.Data.Entity;
using TASVideos.Extensions;
using TASVideos.Filter;
using TASVideos.Models;
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

		[RequirePermission(PermissionTo.EditUsers)]
		public async Task<IActionResult> EditByName(string userName)
		{
			var model = await UserTasks.GetUserForEdit(userName, User.GetUserId());
			return RedirectToPage("/Users/Edit", model);
		}

		[AllowAnonymous]
		[Route("[controller]/[action]/{userName}")]
		public async Task<IActionResult> Profile(string userName)
		{
			var model = await UserTasks.GetUserProfile(userName, includeHidden: false);
			if (model == null)
			{
				return NotFound();
			}

			if (!string.IsNullOrWhiteSpace(model.Signature))
			{
				model.Signature = RenderPost(model.Signature, true, false);
			}

			model.Awards = await _awardTasks.GetAllAwardsForUser(model.Id);

			return View(model);
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
