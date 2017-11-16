using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TASVideos.Data.Entity;
using TASVideos.Filter;
using TASVideos.Models;
using TASVideos.Tasks;

namespace TASVideos.Controllers
{
	[Authorize]
	public class PermissionsController : BaseController
	{
		private readonly PermissionTasks _permissionTasks;

		public PermissionsController(
			PermissionTasks permissionTasks,
			UserTasks userTasks)
			: base(userTasks)
		{
			_permissionTasks = permissionTasks;
		}

		public async Task<IActionResult> Index()
		{
			var model = await _permissionTasks.GetAllPermissionsForDisplay();
			return View(model);
		}

		[RequirePermission(PermissionTo.EditPermissionDetails)]
		public async Task<IActionResult> Edit()
		{
			var model = await _permissionTasks.GetAllPermissionsForEdit();
			return View(model.ToList());
		}

		[HttpPost]
		[ValidateAntiForgeryToken]
		[RequirePermission(PermissionTo.EditPermissionDetails)]
		public async Task<IActionResult> Edit(IEnumerable<PermissionEditViewModel> model)
		{
			if (model == null)
			{
				ModelState.AddModelError("", "No permissions were supplied");
			}

			if (ModelState.IsValid)
			{
				await _permissionTasks.UpdatePermissionDetails(model);
				return RedirectToAction(nameof(Index));
			}

			return View(model?.ToList());
		}
    }
}