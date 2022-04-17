using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TASVideos.Core.Services;

namespace TASVideos.Pages.Account;

[AllowAnonymous]
public class ConfirmEmailChangeModel : BasePageModel
{
	private readonly UserManager _userManager;
	private readonly IUserMaintenanceLogger _userMaintenanceLogger;
	private readonly ICacheService _cache;

	public ConfirmEmailChangeModel(
		UserManager userManager,
		IUserMaintenanceLogger userMaintenanceLogger,
		ICacheService cache)
	{
		_userManager = userManager;
		_userMaintenanceLogger = userMaintenanceLogger;
		_cache = cache;
	}

	public async Task<IActionResult> OnGet(string? userId, string? code)
	{
		if (userId is null || code is null)
		{
			return Home();
		}

		var user = await _userManager.FindByIdAsync(userId);
		if (user is null)
		{
			return Home();
		}

		var cacheResult = _cache.TryGetValue(code, out string newEmail);
		if (!cacheResult)
		{
			return BadRequest("Unrecognized or expired code.");
		}

		var result = await _userManager.SetEmailAsync(user, newEmail);
		if (!result.Succeeded)
		{
			return RedirectToPage("/Error");
		}

		_cache.Remove(code);
		await _userMaintenanceLogger.Log(user.Id, $"User changed email from {IpAddress}");
		return RedirectToPage("/Profile/Settings");
	}
}
