using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TASVideos.Data.Entity;
using TASVideos.Tasks;
using TASVideos.Filter;
using TASVideos.Models;

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

		public IActionResult Index()
		{
			var model = _permissionTasks.GetAllPermissionsForDisplay();
			return View(model);
		}

		[RequirePermission(PermissionTo.EditPermissionDetails)]
		public IActionResult Edit()
		{
			var model = _permissionTasks.GetAllPermissionsForEdit()
				.ToList();
			return View(model);
		}

		[RequirePermission(PermissionTo.EditPermissionDetails)]
		[HttpPost]
		public IActionResult Edit(IEnumerable<PermissionEditViewModel> model)
		{
			if (model == null)
			{
				ModelState.AddModelError("", "No permissions were supplied");
			}

			if (ModelState.IsValid)
			{
				_permissionTasks.UpdatePermissionDetails(model);
				return RedirectToAction(nameof(Index));
			}

			return View(model?.ToList());
		}
    }
}