using System;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.RazorPages;

using TASVideos.Data.Entity;
using TASVideos.Extensions;
using TASVideos.Services;

namespace TASVideos.Pages
{
	[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, Inherited = false)]
	public class SetPageViewBagAttribute : ResultFilterAttribute
	{
		private static readonly Version VersionInfo = Assembly.GetExecutingAssembly().GetName().Version;
		private string Version => $"{VersionInfo.Major}.{VersionInfo.Minor}.{VersionInfo.Revision}";
		
		public override async Task OnResultExecutionAsync(ResultExecutingContext context, ResultExecutionDelegate next)
		{
			if (context.Result is PageResult pageResult)
			{
				var viewData = pageResult.ViewData;
				viewData["Version"] = Version;

				var userClaimsPrincipal = context.HttpContext.User;
				if (userClaimsPrincipal.Identity.IsAuthenticated)
				{
					var userTasks = (UserManager)context.HttpContext.RequestServices.GetService(typeof(UserManager));
					viewData["UserPermissions"] = await userTasks.GetUserPermissionsById(userClaimsPrincipal.GetUserId());
				}
				else
				{
					viewData["UserPermissions"] = Enumerable.Empty<PermissionTo>();
				}
			}

			await next.Invoke();
		}
	}
}
