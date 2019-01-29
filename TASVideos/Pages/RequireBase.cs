using System;
using System.Net;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using TASVideos.Extensions;

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
	}
}
