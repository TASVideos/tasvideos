using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using TASVideos.Data.Entity;
using TASVideos.Filter;
using TASVideos.Models;
using TASVideos.Tasks;

namespace TASVideos.Controllers
{
	public class RolesController : BaseController
	{
		private readonly RoleTasks _roleTasks;

		public RolesController(
			RoleTasks roleTasks,
			UserTasks userTasks)
			: base(userTasks)
		{
			_roleTasks = roleTasks;
		}

		[AllowAnonymous]
		public async Task<IActionResult> Index()
		{
			var model = await _roleTasks.GetAllRolesForDisplayAsync();
			return View(model);
		}

		[RequirePermission(PermissionTo.EditRoles)]
		public async Task<IActionResult> AddEdit(int? id)
		{
			var model = await _roleTasks.GetRoleForEditAsync(id);
			model.AvailablePermissions = PermissionsSelectList;
			return View(model);
		}

		[HttpPost]
		[ValidateAntiForgeryToken]
		[RequirePermission(PermissionTo.EditRoles)]
		public async Task<IActionResult> AddEdit(RoleEditViewModel model)
		{
			if (ModelState.IsValid)
			{
				await _roleTasks.AddUpdateRoleAsync(model);
				return RedirectToAction(nameof(Index));
			}

			model.AvailablePermissions = PermissionsSelectList;
			return View(model);
		}

		[RequirePermission(PermissionTo.DeleteRoles)]
		public async Task<IActionResult> Delete(int id)
		{
			await _roleTasks.DeleteRoleAsync(id);
			return RedirectToAction(nameof(Index));
		}

		/// <summary>
		/// A select list of all available <seealso cref="PermissionTo"/> in the system
		/// </summary>
		private static IEnumerable<SelectListItem> PermissionsSelectList =>
			Enum.GetValues(typeof(PermissionTo))
				.Cast<PermissionTo>()
				.ToList()
				.Select(p => new SelectListItem
				{
					Value = ((int)p).ToString(),
					Text = p.ToString()
				});
	}
}
