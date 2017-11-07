using System;
using System.Collections.Generic;
using System.Linq;

using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Routing;

using TASVideos.Controllers;
using TASVideos.Data;
using TASVideos.Data.Entity;
using TASVideos.Extensions;

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

		public override void OnActionExecuting(ActionExecutingContext context)
		{
			var userClaimsPrincipal = context.HttpContext.User;

			if (!userClaimsPrincipal.Identity.IsAuthenticated)
			{
				context.Result = RedirectHome();
				return;
			}

			var userId = int.Parse(userClaimsPrincipal.Claims
				.First(c => c.Type == "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier").Value);

			var db = (ApplicationDbContext)context.HttpContext.RequestServices.GetService(typeof(ApplicationDbContext));

			var userPerms = db.GetUserPermissionsById(userId);
			var reqs = new HashSet<PermissionTo>(_requiredPermissions);

			if (_matchAny && reqs.Any(r => userPerms.Contains(r)) || reqs.IsSubsetOf(userPerms))
			{
				base.OnActionExecuting(context);
			}
			else if (context.HttpContext.Request.IsAjaxRequest())
			{
				context.HttpContext.Response.StatusCode = 403;
				context.Result = new EmptyResult();
			}
			else
			{
				context.Result = RedirectHome();
			}
		}

		private RedirectToRouteResult RedirectHome()
		{
			return new RedirectToRouteResult(
				new RouteValueDictionary(new { controller = "Home", action = nameof(HomeController.Index) }));
		}
	}
}
