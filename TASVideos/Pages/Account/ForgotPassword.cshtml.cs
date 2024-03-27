using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TASVideos.Core.Services;
using TASVideos.Core.Services.Email;

namespace TASVideos.Pages.Account;

[AllowAnonymous]
public class ForgotPasswordModel(UserManager userManager, IEmailService emailService) : BasePageModel
{
	[BindProperty]
	[EmailAddress]
	public string Email { get; set; } = "";

	public async Task<IActionResult> OnPost()
	{
		if (!ModelState.IsValid)
		{
			return Page();
		}

		var user = await userManager.FindByEmailAsync(Email);
		if (user is null || !await userManager.IsEmailConfirmedAsync(user))
		{
			// Don't reveal that the user does not exist or is not confirmed
			return RedirectToPage("ForgotPasswordConfirmation");
		}

		var code = await userManager.GeneratePasswordResetTokenAsync(user);
		var callbackUrl = Url.ResetPasswordCallbackLink(user.Id.ToString(), code, "https");
		await emailService.ResetPassword(Email, callbackUrl);

		return RedirectToPage("ForgotPasswordConfirmation");
	}
}
