using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using TASVideos.Data.Entity;
using TASVideos.Extensions;
using TASVideos.ForumEngine;
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

		protected UserTasks UserTasks { get; }

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

					_userPermission = UserTasks.GetUserPermissionsById(User.GetUserId());
				}

				return _userPermission;
			}
		}

		protected bool UserHas(PermissionTo permission)
		{
			return UserPermissions.Contains(permission);
		}

		protected IPAddress IpAddress => Request.HttpContext.Connection.RemoteIpAddress;

		protected IActionResult RedirectHome()
		{
			return Redirect("/Home/Index");
		}

		protected IActionResult RedirectAccessDenied()
		{
			return RedirectToAction(nameof(AccountController.AccessDenied), "Account");
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

		protected string RenderHtml(string text)
		{
			return RenderPost(text, false, true);
		}

		protected string RenderPost(string text, bool useBbCode, bool useHtml)
		{
			var parsed = PostParser.Parse(text, useBbCode, useHtml);
			using (var writer = new StringWriter())
			{
				parsed.WriteHtml(writer);
				return writer.ToString();
			}

		}
	}
}