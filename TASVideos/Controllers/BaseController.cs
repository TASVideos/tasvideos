using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Reflection;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using TASVideos.Data.Entity;
using TASVideos.Extensions;
using TASVideos.Tasks;

namespace TASVideos.Controllers
{
	public class BaseController : Controller
	{
		private static readonly Version VersionInfo = Assembly.GetExecutingAssembly().GetName().Version;

		private readonly UserTasks _userTasks;
		private IEnumerable<PermissionTo> _userPermission;

		public BaseController(UserTasks userTasks)
		{
			_userTasks = userTasks;
		}

		internal string Version => $"{VersionInfo.Major}.{VersionInfo.Minor}.{VersionInfo.Revision}";

		internal IEnumerable<PermissionTo> UserPermissions
		{
			get
			{
				if (_userPermission == null)
				{
					if (HttpContext == null || !User.Identity.IsAuthenticated)
					{
						_userPermission = Enumerable.Empty<PermissionTo>();
					}

					_userPermission = _userTasks.GetUserPermissionsById(User.GetUserId());
				}

				return _userPermission;
			}
		}

		protected IPAddress IpAddress => Request.HttpContext.Connection.RemoteIpAddress;

		protected IActionResult RedirectHome()
		{
			return Redirect("/Home/Index");
		}

		protected IActionResult RedirectToLogin()
		{
			return RedirectToAction(nameof(AccountController.Login), "Account");
		}

		protected void AddErrors(IdentityResult result)
		{
			foreach (var error in result.Errors)
			{
				ModelState.AddModelError(string.Empty, error.Description);
			}
		}

		protected IActionResult RedirectToLocal(string returnUrl)
		{
			if (Url.IsLocalUrl(returnUrl))
			{
				return Redirect(returnUrl);
			}

			return RedirectHome();
		}
	}
}