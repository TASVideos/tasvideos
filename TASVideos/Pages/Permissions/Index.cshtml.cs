using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using TASVideos.Models;
using TASVideos.Tasks;

namespace TASVideos.Pages.Permissions
{
	[Authorize]
	public class IndexModel : BasePageModel
	{
		private readonly PermissionTasks _permissionTasks;
		public IndexModel(PermissionTasks permissionsTasks, UserTasks userTasks)
			: base(userTasks)
		{
			_permissionTasks = permissionsTasks;
		}

		public IEnumerable<PermissionDisplayModel> Model { get; set; }

		public async Task OnGetAsync()
		{
			Model = await _permissionTasks.GetAllPermissionsForDisplay();
		}
	}
}
