using System.Net;
using Microsoft.AspNetCore.Mvc.Filters;

namespace TASVideos.Pages;

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, Inherited = false)]
public class RequireBase : Attribute
{
	protected static IActionResult ReRouteToLogin(PageHandlerExecutingContext context)
	{
		var returnUrl = context.HttpContext.Request.Path + context.HttpContext.Request.QueryString;
		return new RedirectToPageResult("/Account/Login", new { returnUrl });
	}

	protected static void Denied(PageHandlerExecutingContext context)
	{
		if (context.HttpContext.Request.IsAjaxRequest())
		{
			context.HttpContext.Response.StatusCode = (int)HttpStatusCode.Forbidden;
			context.Result = new EmptyResult();
		}
		else
		{
			context.Result = new RedirectToPageResult("/Account/AccessDenied");
		}
	}

	protected static async Task<IReadOnlyCollection<PermissionTo>> GetUserPermissions(PageHandlerExecutingContext context)
	{
		// On Post calls, we are potentially changing data, which could be malicious
		// Let's take the database hit to get the most recent permissions rather than relying
		// on the user cookie, in case the user's permissions have recently changed, such as from being "banned"
		// We are assuming we don't have malicious GET calls, and that for GETs we can afford to wait f
		// for the cookie expiration
		if (context.HandlerMethod?.HttpMethod == "Post")
		{
			var userManager = context.HttpContext.RequestServices.GetRequiredService<IUserManager>();
			return await userManager.GetUserPermissionsById(context.HttpContext.User.GetUserId());
		}

		return context.HttpContext.User.Permissions();
	}

	protected static void SetRequiredPermissionsView(PageHandlerExecutingContext context, HashSet<PermissionTo> requiredPermissions, bool matchAny)
	{
		context.HttpContext.SetRequiredPermissionsView(new RequirePermissionsView { Permissions = requiredPermissions, MatchAny = matchAny });
	}
}

public class RequirePermissionsView
{
	public HashSet<PermissionTo> Permissions { get; set; } = [];
	public bool MatchAny { get; set; }
}
