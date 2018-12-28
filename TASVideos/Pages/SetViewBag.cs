using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using TASVideos.Data.Entity;
using TASVideos.Extensions;
using TASVideos.Tasks;

namespace TASVideos.Pages
{
	[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, Inherited = false)]
	public class SetViewBag : ResultFilterAttribute
	{
		private static readonly Version VersionInfo = Assembly.GetExecutingAssembly().GetName().Version;
		private string Version => $"{VersionInfo.Major}.{VersionInfo.Minor}.{VersionInfo.Revision}";
		
		public override async Task OnResultExecutionAsync(ResultExecutingContext context, ResultExecutionDelegate next)
		{
			var viewData = ((PageResult)context.Result).ViewData;
			viewData["Version"] = Version;

			var userClaimsPrincipal = context.HttpContext.User;
			if (userClaimsPrincipal.Identity.IsAuthenticated)
			{
				var userTasks = (UserTasks)context.HttpContext.RequestServices.GetService(typeof(UserTasks));
				viewData["UserPermissions"] = await userTasks.GetUserPermissionsById(userClaimsPrincipal.GetUserId());
			}
			else
			{
				viewData["UserPermissions"] = Enumerable.Empty<PermissionTo>();
			}

			await next.Invoke();
		}
	}
}
