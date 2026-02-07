using System.Net;
using Microsoft.AspNetCore.Mvc.Filters;

namespace TASVideos.Pages;

public class RequireEdit : RequireBase, IAsyncPageFilter
{
	public async Task OnPageHandlerSelectionAsync(PageHandlerSelectedContext context)
	{
		await Task.CompletedTask;
	}

	public async Task OnPageHandlerExecutionAsync(PageHandlerExecutingContext context, PageHandlerExecutionDelegate next)
	{
		var user = context.HttpContext.User;

		if (!user.IsLoggedIn())
		{
			context.Result = ReRouteToLogin(context);
			return;
		}

		var pageToEdit = "";
		if (context.HttpContext.Request.QueryString.Value?.Contains("path=") ?? false)
		{
			pageToEdit = WebUtility.UrlDecode((context.HttpContext.Request.QueryString.Value ?? "path=").Split("path=")[1]);
		}

		var userPerms = await GetUserPermissions(context);
		var canEdit = WikiHelper
			.UserCanEditWikiPage(pageToEdit, user.Name(), userPerms, out var relevantPermissions);

		if (canEdit)
		{
			SetRequiredPermissionsView(context, relevantPermissions, matchAny: true);

			await next.Invoke();
		}
		else
		{
			Denied(context);
		}
	}
}
