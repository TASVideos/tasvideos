using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TASVideos.Tasks;

namespace TASVideos.Controllers
{
	[Authorize]
	public class PermissionsController : BaseController
	{
		private readonly PermissionTasks _permissionTasks;

		public PermissionsController(PermissionTasks permissionTasks)
		{
			_permissionTasks = permissionTasks;
		}

		public IActionResult Index()
		{
			var model = _permissionTasks.GetAllPermissionsForDisplay();

			return View(model);
        }
    }
}