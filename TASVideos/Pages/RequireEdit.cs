using System;
using System.Net;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

using TASVideos.Extensions;
using TASVideos.Tasks;

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
			var userClaimsPrincipal = context.HttpContext.User;

			if (!userClaimsPrincipal.Identity.IsAuthenticated)
			{
				context.Result = ReRouteToLogin(context.HttpContext.Request.Path + context.HttpContext.Request.QueryString);
				return;
			}

			var userId = userClaimsPrincipal.GetUserId();

			var userTasks = (UserTasks)context.HttpContext.RequestServices.GetService(typeof(UserTasks));

			var userPerms = await userTasks.GetUserPermissionsById(userId);

			string pageToEdit = "";
			if (context.HttpContext.Request.QueryString.Value.Contains("path="))
			{
				pageToEdit = WebUtility.UrlDecode((context.HttpContext.Request.QueryString.Value ?? "path=").Split("path=")[1]);
			}

			var canEdit = WikiHelper
				.UserCanEditWikiPage(pageToEdit, userClaimsPrincipal.Identity.Name, userPerms);

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
