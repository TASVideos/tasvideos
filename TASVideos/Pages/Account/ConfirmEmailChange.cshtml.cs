using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TASVideos.Core.Services;

namespace TASVideos.Pages.Account;

[Authorize]
public class ConfirmEmailChangeModel(
	UserManager userManager,
	IUserMaintenanceLogger userMaintenanceLogger,
	ICacheService cache)
	: BasePageModel
{
	public async Task<IActionResult> OnGet(string? code)
	{
		if (string.IsNullOrWhiteSpace(code))
		{
			return AccessDenied();
		}

		var user = await userManager.GetRequiredUser(User);

		var cacheResult = cache.TryGetValue(code, out string newEmail);
		if (!cacheResult)
		{
			return BadRequest("Unrecognized or expired code.");
		}

		var result = await userManager.ChangeEmailAsync(user, newEmail, code);
		if (!result.Succeeded)
		{
			return RedirectToPage("/Error");
		}

		cache.Remove(code);
		await userMaintenanceLogger.Log(user.Id, $"User changed email from {IpAddress}");
		return RedirectToPage("/Profile/Settings");
	}
}
