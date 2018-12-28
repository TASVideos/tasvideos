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

		public string BaseUrl => $"{Request.Scheme}://{Request.Host}{Request.PathBase}";

		internal string Version => $"{VersionInfo.Major}.{VersionInfo.Minor}.{VersionInfo.Revision}";

		internal IEnumerable<PermissionTo> UserPermissions =>
			_userPermission ?? (_userPermission = HttpContext == null || !User.Identity.IsAuthenticated
				? Enumerable.Empty<PermissionTo>()
				: UserTasks.GetUserPermissionsById(User.GetUserId()).Result);

		protected UserTasks UserTasks { get; }
		protected IPAddress IpAddress => Request.HttpContext.Connection.RemoteIpAddress;

		protected bool UserHas(PermissionTo permission)
		{
			return UserPermissions.Contains(permission);
		}

		protected IActionResult RedirectHome()
		{
			return RedirectToPage("/Index");
		}

		protected IActionResult AccessDenied()
		{
			return RedirectToPage("/Account/AccessDenied");
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

		protected string RenderBbcode(string text)
		{
			return RenderPost(text, true, false);
		}

		protected string RenderSignature(string text)
		{
			return RenderBbcode(text); // Bbcode on, Html off hardcoded, do we want this to be configurable?
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