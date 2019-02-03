using System.Net;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Filters;

using TASVideos.Extensions;

namespace TASVideos.Pages
{
	public class RequireEdit : RequireBase, IAsyncPageFilter
	{
		public async Task OnPageHandlerSelectionAsync(PageHandlerSelectedContext context)
		{
			await Task.CompletedTask;
		}

		public async Task OnPageHandlerExecutionAsync(PageHandlerExecutingContext context, PageHandlerExecutionDelegate next)
		{
			var user = context.HttpContext.User;

			if (!user.Identity.IsAuthenticated)
			{
				context.Result = ReRouteToLogin(context);
				return;
			}

			string pageToEdit = "";
			if (context.HttpContext.Request.QueryString.Value.Contains("path="))
			{
				pageToEdit = WebUtility.UrlDecode((context.HttpContext.Request.QueryString.Value ?? "path=").Split("path=")[1]);
			}

			var userPerms = await GetUserPermissions(context);
			var canEdit = WikiHelper
				.UserCanEditWikiPage(pageToEdit, user.Identity.Name, userPerms);

			if (canEdit)
			{
				await next.Invoke();
			}
			else 
			{
				Denied(context);
			}
		}
	}
}
