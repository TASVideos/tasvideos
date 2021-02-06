﻿using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace TASVideos.Pages
{
	public class BasePageModel : PageModel
	{
		protected string IpAddress => Request.HttpContext.Connection.RemoteIpAddress?.ToString() ?? "";

		protected IActionResult Home() => RedirectToPage("/Index");

		protected IActionResult AccessDenied() => RedirectToPage("/Account/AccessDenied");

		protected IActionResult Login() => new RedirectToPageResult("Login");

		protected IActionResult RedirectToLocal(string? returnUrl)
		{
			returnUrl ??= "";
			return Url.IsLocalUrl(returnUrl)
				? LocalRedirect(returnUrl)
				: Home();
		}

		protected void AddErrors(IdentityResult result)
		{
			foreach (var error in result.Errors)
			{
				ModelState.AddModelError(string.Empty, error.Description);
			}
		}
	}
}
