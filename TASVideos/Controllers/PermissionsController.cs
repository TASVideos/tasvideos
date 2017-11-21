using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
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
	}
}