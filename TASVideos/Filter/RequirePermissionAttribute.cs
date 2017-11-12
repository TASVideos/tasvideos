using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Routing;

using TASVideos.Controllers;
using TASVideos.Data.Entity;
using TASVideos.Extensions;
using TASVideos.Tasks;

namespace TASVideos.Filter
{
	[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, Inherited = false)]
	public class RequirePermissionAttribute : ActionFilterAttribute
	{
		private readonly PermissionTo[] _requiredPermissions;
		private readonly bool _matchAny;

		public RequirePermissionAttribute(PermissionTo requiredPermission)
		{
			_requiredPermissions = new[] { requiredPermission };
		}

		public RequirePermissionAttribute(params PermissionTo[] requiredPermissions)
		{
			_requiredPermissions = requiredPermissions;
		}

		public RequirePermissionAttribute(bool matchAny, params PermissionTo[] requiredPermissions)
		{
			_requiredPermissions = requiredPermissions;
			_matchAny = matchAny;
		}

		public override async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
		{
			var userClaimsPrincipal = context.HttpContext.User;

			if (!userClaimsPrincipal.Identity.IsAuthenticated)
			{
				
				context.Result = ReRouteToLogin(context.HttpContext.Request.Path);
				return;
			}

			var userId = userClaimsPrincipal.GetUserId();

			var userTasks = (UserTasks)context.HttpContext.RequestServices.GetService(typeof(UserTasks));

			var userPerms = await userTasks.GetUserPermissionsByIdAsync(userId);
			var reqs = new HashSet<PermissionTo>(_requiredPermissions);

			if (_matchAny && reqs.Any(r => userPerms.Contains(r)) || reqs.IsSubsetOf(userPerms))
			{
				await base.OnActionExecutionAsync(context, next);
			}
			else if (context.HttpContext.Request.IsAjaxRequest())
			{
				context.HttpContext.Response.StatusCode = 403;
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
