using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

using Microsoft.AspNetCore.Mvc;
using TASVideos.Data.Entity;
using TASVideos.Extensions;
using TASVideos.Tasks;

namespace TASVideos.Controllers
{
	public class BaseController : Controller
	{
		private static readonly Version VersionInfo = Assembly.GetExecutingAssembly().GetName().Version;

		private IEnumerable<PermissionTo> _userPermission;

		public BaseController(UserTasks userTasks)
		{
			UserTasks = userTasks;
		}

		internal string Version => $"{VersionInfo.Major}.{VersionInfo.Minor}.{VersionInfo.Revision}";

		internal IEnumerable<PermissionTo> UserPermissions =>
			_userPermission ?? (_userPermission = HttpContext == null || !User.Identity.IsAuthenticated
				? Enumerable.Empty<PermissionTo>()
				: UserTasks.GetUserPermissionsById(User.GetUserId()).Result);

		protected UserTasks UserTasks { get; }
	}
}