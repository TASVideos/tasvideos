using TASVideos.Core.Services.Email;

namespace TASVideos.Pages.Profile;

[Authorize]
public class ChangePasswordModel(IEmailService emailService, ISignInManager signInManager, IUserManager userManager) : BasePageModel
{
	[BindProperty]
	[DataType(DataType.Password)]
	public string CurrentPassword { get; set; } = "";

	[BindProperty]
	[StringLength(128, ErrorMessage = "The {0} must be at least {2} and at max {1} characters long.", MinimumLength = 12)]
	[DataType(DataType.Password)]
	public string NewPassword { get; set; } = "";

	[BindProperty]
	[StringLength(128, ErrorMessage = "The {0} must be at least {2} and at max {1} characters long.", MinimumLength = 12)]
	[DataType(DataType.Password)]
	[Compare("NewPassword", ErrorMessage = "The new password and confirmation password do not match.")]
	public string ConfirmNewPassword { get; set; } = "";

	public async Task<IActionResult> OnGet()
	{
		var hasPassword = await signInManager.HasPassword(User);
		if (!hasPassword)
		{
			return RedirectToPage("SetPassword");
		}

		return Page();
	}

	public async Task<IActionResult> OnPost()
	{
		if (!ModelState.IsValid)
		{
			return Page();
		}

		var user = await userManager.GetRequiredUser(User);

		if (!signInManager.IsPasswordAllowed(user.UserName, user.Email, NewPassword))
		{
			ModelState.AddModelError(nameof(NewPassword), "This password is not allowed, please ensure your password is sufficiently different from your username and/or email");
		}

		if (!ModelState.IsValid)
		{
			return Page();
		}

		var changePasswordResult = await userManager.ChangePassword(user, CurrentPassword, NewPassword);
		if (!changePasswordResult.Succeeded)
		{
			AddErrors(changePasswordResult);
			return Page();
		}

		await signInManager.SignIn(user, isPersistent: false);
		var code = await userManager.GeneratePasswordResetToken(user);
		var callbackUrl = Url.ResetPasswordCallbackLink(user.Id.ToString(), code);
		await emailService.PasswordResetConfirmation(user.Email, callbackUrl);
		SuccessStatusMessage("Your password has been changed.");
		return BasePageRedirect("Index");
	}
}
