using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using TASVideos.Data.Entity;
using TASVideos.Extensions;
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

		/// <summary>
		/// A select list of all available <seealso cref="PermissionTo"/> in the system
		/// </summary>
		private static IEnumerable<SelectListItem> PermissionsSelectList =>
			Enum.GetValues(typeof(PermissionTo))
				.Cast<PermissionTo>()
				.Select(p => new SelectListItem
				{
					Value = ((int)p).ToString(),
					Text = p.EnumDisplayName()
				})
				.ToList();

		[AllowAnonymous]
		public async Task<IActionResult> Index(string role)
		{
			if (string.IsNullOrWhiteSpace(role))
			{
				return RedirectToAction(nameof(List));
			}

			var model = await _roleTasks.GetRoleForDisplay(role);

			if (model == null)
			{
				return NotFound();
			}

			ViewData["Title"] = "Role: " + role;
			return View("_Role", model);
		}

		[AllowAnonymous]
		public async Task<IActionResult> List()
		{
			var model = await _roleTasks.GetAllRolesForDisplay();
			return View(model);
		}

		[RequirePermission(PermissionTo.EditRoles)]
		public async Task<IActionResult> AddEdit(int? id)
		{
			var model = await _roleTasks.GetRoleForEdit(id);

			if (model == null)
			{
				return NotFound();
			}

			model.AvailablePermissions = PermissionsSelectList;
			model.AvailableAssignablePermissions = model.SelectedPermissions
				.Select(sp => new SelectListItem
				{
					Text = ((PermissionTo)sp).ToString(),
					Value = sp.ToString()
				});
			return View(model);
		}

		[HttpPost, ValidateAntiForgeryToken]
		[RequirePermission(PermissionTo.EditRoles)]
		public async Task<IActionResult> AddEdit(RoleEditModel model)
		{
			model.Links = model.Links.Where(l => !string.IsNullOrWhiteSpace(l));
			if (ModelState.IsValid)
			{
				await _roleTasks.AddUpdateRole(model);
				return RedirectToAction(nameof(Index));
			}

			model.AvailablePermissions = PermissionsSelectList;
			model.AvailableAssignablePermissions = model.SelectedPermissions
				.Select(sp => new SelectListItem
				{
					Text = ((PermissionTo)sp).ToString(),
					Value = sp.ToString()
				});
			return View(model);
		}

		[RequirePermission(PermissionTo.DeleteRoles)]
		public async Task<IActionResult> Delete(int id)
		{
			await _roleTasks.DeleteRole(id);
			return RedirectToAction(nameof(Index));
		}

		[RequirePermission(PermissionTo.EditRoles)]
		public async Task<IActionResult> RolesThatCanBeAssignedBy(int[] ids)
		{
			var result = await _roleTasks.RolesThatCanBeAssignedBy(ids.Select(p => (PermissionTo)p));
			return Json(result);
		}
	}
}
