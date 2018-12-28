using System.Linq;
using System.Threading.Tasks;

using Microsoft.AspNetCore.Mvc;

using TASVideos.Data.Entity;
using TASVideos.Filter;
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

		[RequirePermission(PermissionTo.DeleteRoles)]
		public async Task<IActionResult> Delete(int id)
		{
			await _roleTasks.DeleteRole(id);
			return RedirectToPage("Roles/Index");
		}

		[RequirePermission(PermissionTo.EditRoles)]
		public async Task<IActionResult> RolesThatCanBeAssignedBy(int[] ids)
		{
			var result = await _roleTasks.RolesThatCanBeAssignedBy(ids.Select(p => (PermissionTo)p));
			return Json(result);
		}
	}
}
