using System;
using System.Collections.Generic;
using System.Linq;
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
	public class RequirePermissionAttribute : Attribute, IAsyncPageFilter
	{
		public RequirePermissionAttribute(PermissionTo requiredPermission)
		{
			RequiredPermissions = new[] { requiredPermission };
		}

		public RequirePermissionAttribute(params PermissionTo[] requiredPermissions)
		{
			RequiredPermissions = requiredPermissions;
		}

		public RequirePermissionAttribute(bool matchAny, params PermissionTo[] requiredPermissions)
		{
			MatchAny = matchAny;
			RequiredPermissions = requiredPermissions;
		}

		public bool MatchAny { get; }
		public PermissionTo[] RequiredPermissions { get; }

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

			var userTasks = (UserManager)context.HttpContext.RequestServices.GetService(typeof(UserManager));

			var userPerms = await userTasks.GetUserPermissionsById(userId);
			var perms = new HashSet<PermissionTo>(RequiredPermissions);

			if ((MatchAny && perms.Any(r => userPerms.Contains(r)))
				|| perms.IsSubsetOf(userPerms))
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
			return new RedirectToPageResult("/Account/Login", new { returnUrl });
		}

		private IActionResult AccessDenied()
		{
			return new RedirectToPageResult("/Account/AccessDenied");
		}
	}
}
