using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TASVideos.Core.Services;
using TASVideos.Core.Services.ExternalMediaPublisher;

namespace TASVideos.Pages.Account;

[AllowAnonymous]
public class ConfirmEmailModel : BasePageModel
{
	private readonly UserManager _userManager;
	private readonly SignInManager _signInManager;
	private readonly ExternalMediaPublisher _publisher;
	private readonly IUserMaintenanceLogger _userMaintenanceLogger;

	public ConfirmEmailModel(
		UserManager userManager,
		SignInManager signInManager,
		ExternalMediaPublisher publisher,
		IUserMaintenanceLogger userMaintenanceLogger)
	{
		_userManager = userManager;
		_signInManager = signInManager;
		_publisher = publisher;
		_userMaintenanceLogger = userMaintenanceLogger;
	}

	public async Task<IActionResult> OnGet(string? userId, string? code)
	{
		if (userId == null || code == null)
		{
			return Home();
		}

		var user = await _userManager.FindByIdAsync(userId);
		if (user == null)
		{
			return Home();
		}

		if (user.EmailConfirmed)
		{
			// If user has already clicked the email link, no reason to do all the work of confirming
			return Home();
		}

		var result = await _userManager.ConfirmEmailAsync(user, code);
		if (!result.Succeeded)
		{
			return RedirectToPage("/Error");
		}

		await _userManager.AddStandardRoles(user.Id);
		await _userManager.AddUserPermissionsToClaims(user);
		await _signInManager.SignInAsync(user, isPersistent: false);
		await _publisher.SendUserManagement(
			$"User {user.UserName} activated",
			"",
			$"Users/Profile/{user.UserName}");
		await _userMaintenanceLogger.Log(user.Id, $"User activated from {IpAddress}");
		return Page();
	}
}
