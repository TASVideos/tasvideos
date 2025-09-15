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

		await FirstTimeConfirmation(user, userManager, signInManager, publisher, userMaintenanceLogger, tasVideoAgent, IpAddress);
		return Page();
	}

	public static async Task FirstTimeConfirmation(User user, IUserManager userManager, ISignInManager signInManager, IExternalMediaPublisher publisher, IUserMaintenanceLogger userMaintenanceLogger, ITASVideoAgent tasVideoAgent, string ipAddress)
	{
		await userManager.AddStandardRoles(user.Id);
		await userManager.AddUserPermissionsToClaims(user);
		await signInManager.SignIn(user, isPersistent: false);
		await publisher.SendUserManagement($"User [{user.UserName}]({{0}}) activated", user.UserName);
		await userMaintenanceLogger.Log(user.Id, $"User activated from {ipAddress}");
		await tasVideoAgent.SendWelcomeMessage(user.Id);
	}
}
