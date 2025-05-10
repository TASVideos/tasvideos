namespace TASVideos.Pages.Account;

[AllowAnonymous]
public class ConfirmEmailModel(
	ISignInManager signInManager,
	IUserManager userManager,
	IExternalMediaPublisher publisher,
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

		var user = await userManager.FindById(userId);
		if (user is null)
		{
			return Home();
		}

		if (user.EmailConfirmed)
		{
			// If the user has already clicked the email link, no reason to do all the work of confirming
			return Home();
		}

		var result = await userManager.ConfirmEmail(user, code);
		if (!result.Succeeded)
		{
			return RedirectToPage("/Error");
		}

		await userManager.AddStandardRoles(user.Id);
		await userManager.AddUserPermissionsToClaims(user);
		await signInManager.SignIn(user, isPersistent: false);
		await publisher.SendUserManagement($"User [{user.UserName}]({{0}}) activated", user.UserName);
		await userMaintenanceLogger.Log(user.Id, $"User activated from {IpAddress}");
		await tasVideoAgent.SendWelcomeMessage(user.Id);
		return Page();
	}
}
