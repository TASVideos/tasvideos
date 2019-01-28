using System;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.RazorPages;

using TASVideos.Data.Entity;

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

				var user = context.HttpContext.User;
				if (user.Identity.IsAuthenticated)
				{
					viewData["UserPermissions"] = user.Permissions();
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
