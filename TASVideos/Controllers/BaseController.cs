using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using TASVideos.Data.Entity;
using TASVideos.Extensions;
using TASVideos.Tasks;

namespace TASVideos.Controllers
{
	public class BaseController : Controller
	{
		private readonly UserTasks _userTasks;

		public BaseController(UserTasks userTasks)
		{
			_userTasks = userTasks;
		}

		public IEnumerable<PermissionTo> UserPermissions
		{
			get
			{
				if (HttpContext == null || !User.Identity.IsAuthenticated)
				{
					return Enumerable.Empty<PermissionTo>();
				}

				return _userTasks.GetUserPermissionsById(User.GetUserId());
			}
		}

		protected IActionResult RedirectHome()
		{
			return RedirectToAction(nameof(HomeController.Index), "Home");
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