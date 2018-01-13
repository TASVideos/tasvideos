using System;
using System.Net;
using System.Threading.Tasks;

using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Routing;

using TASVideos.Controllers;
using TASVideos.Extensions;
using TASVideos.Tasks;

namespace TASVideos.Filter
{
	[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, Inherited = false)]
	public class RequireEditAttribute : ActionFilterAttribute
	{
		public override async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
		{
			var userClaimsPrincipal = context.HttpContext.User;

			if (!userClaimsPrincipal.Identity.IsAuthenticated)
			{
				context.Result = ReRouteToLogin(context.HttpContext.Request.Path + context.HttpContext.Request.QueryString);
				return;
			}

			var userId = userClaimsPrincipal.GetUserId();

			var userTasks = (UserTasks)context.HttpContext.RequestServices.GetService(typeof(UserTasks));

			var userPerms = await userTasks.GetUserPermissionsByIdAsync(userId);

			string pageToEdit = "";
			if (context.HttpContext.Request.Method == "GET" && context.HttpContext.Request.QueryString.Value.Contains("path="))
			{
				pageToEdit = WebUtility.UrlDecode((context.HttpContext.Request.QueryString.Value ?? "path=").Split("path=")[1]);
			}
			else if (context.HttpContext.Request.Method == "POST")
			{
				pageToEdit = context.HttpContext.Request.Form["PageName"];
			}

			var canEdit = WikiHelper.UserCanEditWikiPage(pageToEdit, userClaimsPrincipal.Identity.Name, userPerms);

			if (canEdit)
			{
				await base.OnActionExecutionAsync(context, next);
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

		private RedirectToRouteResult ReRouteToLogin(string returnUrl)
		{
			return new RedirectToRouteResult(
				new RouteValueDictionary(new
				{
					controller = "Account",
					action = nameof(AccountController.Login),
					returnUrl
				}));
		}

		private RedirectToRouteResult AccessDenied()
		{
			return new RedirectToRouteResult(
				new RouteValueDictionary(new { controller = "Account", action = nameof(AccountController.AccessDenied) }));
		}
	}
}
