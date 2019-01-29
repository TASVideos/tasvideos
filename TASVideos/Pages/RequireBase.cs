using System;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;

using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

using TASVideos.Data.Entity;
using TASVideos.Extensions;
using TASVideos.Services;

namespace TASVideos.Pages
{
	[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, Inherited = false)]
	public class RequireBase : Attribute
	{
		protected IActionResult ReRouteToLogin(PageHandlerExecutingContext context)
		{
			var returnUrl = context.HttpContext.Request.Path + context.HttpContext.Request.QueryString;
			return new RedirectToPageResult("/Account/Login", new { returnUrl });
		}

		protected IActionResult AccessDenied()
		{
			return new RedirectToPageResult("/Account/AccessDenied");
		}

		protected void Denied(PageHandlerExecutingContext context)
		{
			if (context.HttpContext.Request.IsAjaxRequest())
			{
				context.HttpContext.Response.StatusCode = (int)HttpStatusCode.Forbidden;
				context.Result = new EmptyResult();
			}
			else
			{
				context.Result = AccessDenied();
			}
		}

		protected async Task<IEnumerable<PermissionTo>> GetUserPermissions(PageHandlerExecutingContext context)
		{
			// On Post calls, we are potentially changing data, which could be malicious
			// Let's take the database hit to get the most recent permissions rather than relying
			// on the user cookie, in case the user's permissions have recently changed, such as from being "banned"
			// We are assuming we don't have malicious GET calls, and that for GETs we can afford to wait f
			// for the cookie expiration
			if (context.HandlerMethod.HttpMethod == "Post")
			{
				var userManager = (UserManager)context.HttpContext.RequestServices.GetService(typeof(UserManager));
				// TODO: take these permissions and refresh the cookie's claims while we are at it
				return await userManager.GetUserPermissionsById(context.HttpContext.User.GetUserId());
			}

			return context.HttpContext.User.Permissions();
		}
	}
}
