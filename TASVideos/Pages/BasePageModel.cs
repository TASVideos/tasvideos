using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Threading.Tasks;

using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.RazorPages;

using TASVideos.Controllers;
using TASVideos.Data.Entity;
using TASVideos.Extensions;
using TASVideos.Tasks;

namespace TASVideos.Pages
{
	public class BasePageModel : PageModel
	{
		private static readonly Version VersionInfo = Assembly.GetExecutingAssembly().GetName().Version;

		private IEnumerable<PermissionTo> _userPermission;

		public BasePageModel(UserTasks userTasks)
		{
			UserTasks = userTasks;
		}

		public string BaseUrl => $"{Request.Scheme}://{Request.Host}{Request.PathBase}";

		internal string Version => $"{VersionInfo.Major}.{VersionInfo.Minor}.{VersionInfo.Revision}";

		internal IEnumerable<PermissionTo> UserPermissions =>
			_userPermission ?? (_userPermission = HttpContext == null || !User.Identity.IsAuthenticated
				? Enumerable.Empty<PermissionTo>()
				: UserTasks.GetUserPermissionsById(User.GetUserId()).Result);

		protected UserTasks UserTasks { get; }
		protected IPAddress IpAddress => Request.HttpContext.Connection.RemoteIpAddress;

		public override void OnPageHandlerExecuting(PageHandlerExecutingContext context)
		{
			ViewData["UserPermissions"] = UserPermissions;
			ViewData["Version"] = Version;
		}

		public IActionResult AccessDenied()
		{
			return RedirectToAction($"{nameof(AccountController.AccessDenied)}", "Account");
		}

		protected bool UserHas(PermissionTo permission)
		{
			return UserPermissions.Contains(permission);
		}
	}

}
