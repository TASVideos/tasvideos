﻿using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TASVideos.Core.Services;
using TASVideos.Core.Services.ExternalMediaPublisher;

namespace TASVideos.Pages.Account;

[AllowAnonymous]
public class ConfirmEmailModel(
	UserManager userManager,
	SignInManager signInManager,
	ExternalMediaPublisher publisher,
	IUserMaintenanceLogger userMaintenanceLogger,
	ITASVideoAgent tasVideoAgent)
	: BasePageModel
{
	public async Task<IActionResult> OnGet(string? userId, string? code)
	{
		if (userId is null || code is null)
		{
			return Home();
		}

		var user = await userManager.FindByIdAsync(userId);
		if (user is null)
		{
			return Home();
		}

		if (user.EmailConfirmed)
		{
			// If user has already clicked the email link, no reason to do all the work of confirming
			return Home();
		}

		var result = await userManager.ConfirmEmailAsync(user, code);
		if (!result.Succeeded)
		{
			return RedirectToPage("/Error");
		}

		await userManager.AddStandardRoles(user.Id);
		await userManager.AddUserPermissionsToClaims(user);
		await signInManager.SignInAsync(user, isPersistent: false);
		await publisher.SendUserManagement(
			$"User {user.UserName} activated",
			$"User [{user.UserName}]({{0}}) activated",
			"",
			$"Users/Profile/{Uri.EscapeDataString(user.UserName)}");
		await userMaintenanceLogger.Log(user.Id, $"User activated from {IpAddress}");
		await tasVideoAgent.SendWelcomeMessage(user.Id);
		return Page();
	}
}
