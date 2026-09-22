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
