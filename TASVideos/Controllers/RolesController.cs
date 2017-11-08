using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using TASVideos.Data.Entity;
using TASVideos.Tasks;
using TASVideos.Filter;
using TASVideos.Models;

namespace TASVideos.Controllers
{
    public class RolesController : BaseController
    {
		private readonly RoleTasks _roleTasks;
		private readonly PermissionTasks _permissionTasks;

		public RolesController(
			RoleTasks roleTasks,
			PermissionTasks permissionTasks,
			UserTasks userTasks
			)
			: base(userTasks)
		{
			_roleTasks = roleTasks;
			_permissionTasks = permissionTasks;
		}

		[AllowAnonymous]
		public IActionResult Index()
		{
			var model = _roleTasks.GetAllRolesForDisplay();
			return View(model);
		}

		[RequirePermission(PermissionTo.EditRoles)]
		public IActionResult AddEdit(int? id)
		{
			var model = _roleTasks.GetRoleForEdit(id);
			return View(model);
		}

		[HttpPost]
		[RequirePermission(PermissionTo.EditRoles)]
		public IActionResult AddEdit(RoleEditViewModel model)
		{
			if (ModelState.IsValid)
			{
				_roleTasks.AddUpdateRole(model);
				return RedirectToAction(nameof(Index));
			}

			model.AvailablePermissions = _roleTasks.PermissionsSelectList;
			return View(model);
		}
	}
}
