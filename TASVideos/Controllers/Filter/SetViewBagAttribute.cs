using System;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

using TASVideos.Data.Entity;
using TASVideos.Extensions;
using TASVideos.Tasks;

namespace TASVideos.Controllers.Filter
{
	/// <summary>
	/// Adds user properties to ViewBag/ViewData that have general purposes usage throughout any view
	/// such as User information
	/// </summary>
	[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, Inherited = false)]
	public class SetControllerViewBagAttribute : ActionFilterAttribute
	{
		private static readonly Version VersionInfo = Assembly.GetExecutingAssembly().GetName().Version;
		private string Version => $"{VersionInfo.Major}.{VersionInfo.Minor}.{VersionInfo.Revision}";

		public override async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
		{
			if (context.Controller is Controller controller)
			{
				controller.ViewData["Version"] = Version;
				
				var userClaimsPrincipal = context.HttpContext.User;
				if (userClaimsPrincipal.Identity.IsAuthenticated)
				{
					var userTasks = (UserTasks)context.HttpContext.RequestServices.GetService(typeof(UserTasks));
					controller.ViewData["UserPermissions"] = await userTasks.GetUserPermissionsById(userClaimsPrincipal.GetUserId());
				}
				else
				{
					controller.ViewData["UserPermissions"] = Enumerable.Empty<PermissionTo>();
				}
			}

			await base.OnActionExecutionAsync(context, next);
		}
	}
}
