using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TASVideos.Data.Entity;
using TASVideos.Tasks;
using TASVideos.Filter;

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
			var model = _permissionTasks.GetAllPermissionsForDisplay();
			return View(model);
		}
    }
}