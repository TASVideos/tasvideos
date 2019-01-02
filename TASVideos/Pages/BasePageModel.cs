using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;

using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

using TASVideos.Data.Entity;
using TASVideos.Extensions;
using TASVideos.ForumEngine;
using TASVideos.Tasks;

namespace TASVideos.Pages
{
	public class BasePageModel : PageModel
	{
		private IEnumerable<PermissionTo> _userPermission;

		public BasePageModel(UserTasks userTasks)
		{
			UserTasks = userTasks;
		}

		public string BaseUrl => $"{Request.Scheme}://{Request.Host}{Request.PathBase}";

		internal IEnumerable<PermissionTo> UserPermissions =>
			_userPermission ?? (_userPermission = HttpContext == null || !User.Identity.IsAuthenticated
				? Enumerable.Empty<PermissionTo>()
				: UserTasks.GetUserPermissionsById(User.GetUserId()).Result);

		protected UserTasks UserTasks { get; }
		protected IPAddress IpAddress => Request.HttpContext.Connection.RemoteIpAddress;

		protected IActionResult Home()
		{
			return RedirectToPage("/Index");
		}

		protected IActionResult AccessDenied()
		{
			return RedirectToPage("/Account/AccessDenied");
		}

		protected IActionResult Login()
		{
			return new RedirectToPageResult("Login");
		}

		protected IActionResult RedirectToLocal(string returnUrl)
		{
			if (Url.IsLocalUrl(returnUrl))
			{
				return LocalRedirect(returnUrl);
			}

			return Home();
		}

		protected void AddErrors(IdentityResult result)
		{
			foreach (var error in result.Errors)
			{
				ModelState.AddModelError(string.Empty, error.Description);
			}
		}

		protected bool UserHas(PermissionTo permission)
		{
			return UserPermissions.Contains(permission);
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

		protected string RenderHtml(string text)
		{
			return RenderPost(text, false, true);
		}
	}
}
