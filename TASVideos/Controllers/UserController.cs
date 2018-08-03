using System.Collections.Generic;
using System.Threading.Tasks;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

using TASVideos.Data;
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

		[RequirePermission(true, PermissionTo.ViewPrivateUserData, PermissionTo.EditUsers)]
		public IActionResult List(PagedModel getModel)
		{
			var model = UserTasks.GetPageOfUsers(getModel);
			return View(model);
		}

		[RequirePermission(true, PermissionTo.ViewPrivateUserData, PermissionTo.EditUsers)]
		public async Task<IActionResult> Index(int? id)
		{
			if (!id.HasValue)
			{
				return RedirectToAction(nameof(List));
			}

			var model = await UserTasks.GetUserDetails(id.Value);
			return View(model);
		}

		[RequirePermission(PermissionTo.EditUsers)]
		public async Task<IActionResult> Edit(int id)
		{
			var userName = await UserTasks.GetUserNameById(id);

			if (userName == null)
			{
				return NotFound();
			}

			var model = await UserTasks.GetUserForEdit(userName, User.GetUserId());
			return View(model);
		}

		[RequirePermission(PermissionTo.EditUsers)]
		public async Task<IActionResult> EditByName(string userName)
		{
			var model = await UserTasks.GetUserForEdit(userName, User.GetUserId());
			return View(nameof(Edit), model);
		}

		[HttpPost, ValidateAntiForgeryToken]
		[RequirePermission(PermissionTo.EditUsers)]
		public async Task<IActionResult> Edit(UserEditViewModel model)
		{
			if (!ModelState.IsValid)
			{	
				model.AvailableRoles = await UserTasks.GetAllRolesUserCanAssign(User.GetUserId(), model.SelectedRoles);
				return View(model);
			}

			await UserTasks.EditUser(model);
			return RedirectToAction(nameof(List));
		}

		[RequirePermission(PermissionTo.EditUsers)]
		public async Task<IActionResult> Unlock(int id, string returnUrl)
		{
			await UserTasks.UnlockUser(id);
			return RedirectToLocal(returnUrl);
		}

		[RequirePermission(PermissionTo.EditUsersUserName)]
		public async Task<IActionResult> VerifyUserNameIsUnique(string userName)
		{
			var exists = await UserTasks.CheckUserNameExists(userName);
			return Json(exists);
		}

		[RequirePermission(true, PermissionTo.ViewPrivateUserData, PermissionTo.EditUsers)]
		public async Task<IActionResult> SearchUserName(string partial)
		{
			if (!string.IsNullOrWhiteSpace(partial) && partial.Length > 1)
			{
				var matches = await UserTasks.GetUsersByPartial(partial);
				return Json(matches);
			}

			return Json(new List<string>());
		}

		[AllowAnonymous]
		public async Task<IActionResult> Profile(int id)
		{
			var model = await UserTasks.GetUserProfile(id);
			if (model == null)
			{
				return NotFound();
			}

			if (!string.IsNullOrWhiteSpace(model.Signature))
			{
				model.Signature = RenderPost(model.Signature, true, false);
			}

			model.Awards = await _awardTasks.GetAllAwardsForUser(id);

			return View(model);
		}
	}
}
