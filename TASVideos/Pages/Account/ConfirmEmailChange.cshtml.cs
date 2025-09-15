namespace TASVideos.Pages.Account;

[Authorize]
public class ConfirmEmailChangeModel(
	IUserManager userManager,
	IUserMaintenanceLogger userMaintenanceLogger,
	ICacheService cache,
	ISignInManager signInManager,
	IExternalMediaPublisher publisher,
	ITASVideoAgent tasVideoAgent)
	: BasePageModel
{
	public async Task<IActionResult> OnGet(string? code)
	{
		if (string.IsNullOrWhiteSpace(code))
		{
			return AccessDenied();
		}

		var cacheResult = cache.TryGetValue(code, out string newEmail);
		if (!cacheResult)
		{
			return BadRequest("Unrecognized or expired code.");
		}

		var user = await userManager.GetRequiredUser(User);
		var wasConfirmedPreviously = user.EmailConfirmed;

		var result = await userManager.ChangeEmail(user, newEmail, code);
		if (!result.Succeeded)
		{
			return RedirectToPage("/Error");
		}

		SuccessStatusMessage("Successfully changed email.");

		cache.Remove(code);
		if (wasConfirmedPreviously)
		{
			await userMaintenanceLogger.Log(user.Id, $"User changed email from {IpAddress}");
		}
		else
		{
			await ConfirmEmailModel.FirstTimeConfirmation(user, userManager, signInManager, publisher, userMaintenanceLogger, tasVideoAgent, IpAddress);
		}

		return RedirectToPage("/Profile/Settings");
	}
}
