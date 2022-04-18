using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TASVideos.Core.Services;

namespace TASVideos.Pages.Account;

[Authorize]
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

	public async Task<IActionResult> OnGet(string? code)
	{
		if (string.IsNullOrWhiteSpace(code))
		{
			return AccessDenied();
		}

		var user = await _userManager.GetUserAsync(User);

		var cacheResult = _cache.TryGetValue(code, out string newEmail);
		if (!cacheResult)
		{
			return BadRequest("Unrecognized or expired code.");
		}

		var result = await _userManager.ChangeEmailAsync(user, newEmail, code);
		if (!result.Succeeded)
		{
			return RedirectToPage("/Error");
		}

		_cache.Remove(code);
		await _userMaintenanceLogger.Log(user.Id, $"User changed email from {IpAddress}");
		return RedirectToPage("/Profile/Settings");
	}
}
