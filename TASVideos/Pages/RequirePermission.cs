﻿using Microsoft.AspNetCore.Mvc.Filters;

namespace TASVideos.Pages;

public class RequirePermissionAttribute : RequireBase, IAsyncPageFilter
{
	public RequirePermissionAttribute(PermissionTo requiredPermission)
	{
		RequiredPermissions = [requiredPermission];
	}

	public RequirePermissionAttribute(bool matchAny, params PermissionTo[] requiredPermissions)
	{
		MatchAny = matchAny;
		RequiredPermissions = requiredPermissions.ToHashSet();
	}

	public bool MatchAny { get; }
	public HashSet<PermissionTo> RequiredPermissions { get; }

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

		var userPerms = await GetUserPermissions(context);

		if ((MatchAny && RequiredPermissions.Any(r => userPerms.Contains(r)))
			|| RequiredPermissions.IsSubsetOf(userPerms))
		{
			SetRequiredPermissionsView(context, RequiredPermissions, MatchAny);
			await next.Invoke();
		}
		else
		{
			Denied(context);
		}
	}
}
