using System;
using System.Net;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

using TASVideos.Extensions;

namespace TASVideos.Pages
{
	[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, Inherited = false)]
	public class RequireEdit : Attribute, IAsyncPageFilter
	{
		public async Task OnPageHandlerSelectionAsync(PageHandlerSelectedContext context)
		{
			await Task.CompletedTask;
		}

		public async Task OnPageHandlerExecutionAsync(PageHandlerExecutingContext context, PageHandlerExecutionDelegate next)
		{
			var user = context.HttpContext.User;

			if (!user.Identity.IsAuthenticated)
			{
				context.Result = ReRouteToLogin(context.HttpContext.Request.Path + context.HttpContext.Request.QueryString);
				return;
			}

			string pageToEdit = "";
			if (context.HttpContext.Request.QueryString.Value.Contains("path="))
			{
				pageToEdit = WebUtility.UrlDecode((context.HttpContext.Request.QueryString.Value ?? "path=").Split("path=")[1]);
			}

			var canEdit = WikiHelper
				.UserCanEditWikiPage(pageToEdit, user.Identity.Name, user.Permissions());

			if (canEdit)
			{
				await next.Invoke();
			}
			else if (context.HttpContext.Request.IsAjaxRequest())
			{
				context.HttpContext.Response.StatusCode = (int)HttpStatusCode.Forbidden;
				context.Result = new EmptyResult();
			}
			else
			{
				context.Result = AccessDenied();
			}
		}

		private IActionResult ReRouteToLogin(string returnUrl)
		{
			return new RedirectToPageResult("/Account/Login", returnUrl);
		}

		private IActionResult AccessDenied()
		{
			return new RedirectToPageResult("/Account/AccessDenied");
		}
	}
}
