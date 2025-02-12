using TASVideos.Core.Services.Email;

namespace TASVideos.Pages.Account;

[AllowAnonymous]
public class ForgotPasswordModel : BasePageModel
{
	[BindProperty]
	[EmailAddress]
	public string Email { get; set; } = "";

	public async Task<IActionResult> OnPost(
		[FromServices] UserManager userManager, [FromServices] IEmailService emailService)
	{
		if (!ModelState.IsValid)
		{
			return Page();
		}

		var user = await userManager.FindByEmailAsync(Email);
		if (user is null)
		{
			// Don't reveal that the user does not exist
			return RedirectToPage("ForgotPasswordConfirmation");
		}

		var code = await userManager.GeneratePasswordResetTokenAsync(user);
		var callbackUrl = Url.ResetPasswordCallbackLink(user.Id.ToString(), code);
		await emailService.ResetPassword(Email, callbackUrl, user.UserName);

		return RedirectToPage("ForgotPasswordConfirmation");
	}
}
