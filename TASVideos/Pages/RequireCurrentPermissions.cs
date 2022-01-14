using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.DependencyInjection;
using TASVideos.Core.Services;

namespace TASVideos.Pages
{
	/// <summary>
	/// Ensures the user's permissions will be read from the database
	/// Should be used on any GET requests that have potentially sensitive data
	/// that the user could have been banned from
	/// </summary>
	[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, Inherited = false)]
	public class RequireCurrentPermissions : Attribute, IAsyncPageFilter
	{
		public async Task OnPageHandlerSelectionAsync(PageHandlerSelectedContext context)
		{
			await Task.CompletedTask;
		}

		public async Task OnPageHandlerExecutionAsync(PageHandlerExecutingContext context, PageHandlerExecutionDelegate next)
		{
			var user = context.HttpContext.User;

			if (user.IsLoggedIn())
			{
				var userManager = context.HttpContext.RequestServices.GetRequiredService<UserManager>();
				var userEntity = await userManager.GetUserAsync(user);
				var claims = await userManager.AddUserPermissionsToClaims(userEntity);

				user.ReplacePermissionClaims(claims);
			}

			await next.Invoke();
		}
	}
}
