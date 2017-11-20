using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using TASVideos.Data.Entity;
using TASVideos.Filter;
using TASVideos.Models;
using TASVideos.Tasks;

namespace TASVideos.Controllers
{
	[RequirePermission(true, PermissionTo.ViewUsers, PermissionTo.EditUsers)]
	public class UserController : BaseController
	{
		private readonly UserTasks _userTasks;

		public UserController(
			UserTasks userTasks)
			: base(userTasks)
		{
			_userTasks = userTasks;
		}

		public IActionResult List()
		{
			var model = _userTasks.GetPageOfUsers(new PagedModel
			{
				CurrentPage = 1,
				PageSize = 20
			});

			return View(model);
		}

		public async Task<IActionResult> Index(int? id)
		{
			if (!id.HasValue)
			{
				return RedirectToAction(nameof(List));
			}

			var model = await _userTasks.GetUserDetails(id.Value);
			return View(model);
		}

		[RequirePermission(PermissionTo.EditUsers)]
		public async Task<IActionResult> Edit(int id)
		{
			var model = await _userTasks.GetUserForEdit(id, UserPermissions);
			return View(model);
		}

		[HttpPost]
		[ValidateAntiForgeryToken]
		[RequirePermission(PermissionTo.EditUsers)]
		public async Task<IActionResult> Edit(UserEditViewModel model)
		{
			if (!ModelState.IsValid)
			{
				model.AvailableRoles = await _userTasks.GetAllRolesUserCanAssign(UserPermissions);
				return View(model);
			}

			await _userTasks.EditUser(model);
			return RedirectToAction(nameof(List));
		}

		[RequirePermission(PermissionTo.EditUsers)]
		public async Task<IActionResult> Unlock(int id, string returnUrl)
		{
			await _userTasks.UnlockUser(id);
			return RedirectToLocal(returnUrl);
		}

		[RequirePermission(PermissionTo.EditUsersUserName)]
		public async Task<IActionResult> VerifyUserNameIsUnique(string userName)
		{
			var exists = await _userTasks.CheckUserNameExists(userName);
			return Json(exists);
		}
	}
}
