using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TASVideos.Core.Services;
using TASVideos.Core.Services.Email;

namespace TASVideos.Pages.Profile;

[Authorize]
public class ChangePasswordModel : BasePageModel
{
	private readonly IEmailService _emailService;
	private readonly SignInManager _signInManager;
	private readonly UserManager _userManager;

	public ChangePasswordModel(
		IEmailService emailService,
		SignInManager signInManager,
		UserManager userManager)
	{
		_emailService = emailService;
		_signInManager = signInManager;
		_userManager = userManager;
	}

	[BindProperty]
	[Required]
	[DataType(DataType.Password)]
	[Display(Name = "Current password")]
	public string OldPassword { get; set; } = "";

	[BindProperty]
	[Required]
	[StringLength(128, ErrorMessage = "The {0} must be at least {2} and at max {1} characters long.", MinimumLength = 12)]
	[DataType(DataType.Password)]
	[Display(Name = "New password")]
	public string NewPassword { get; set; } = "";

	[BindProperty]
	[Required]
	[StringLength(128, ErrorMessage = "The {0} must be at least {2} and at max {1} characters long.", MinimumLength = 12)]
	[DataType(DataType.Password)]
	[Display(Name = "Confirm new password")]
	[Compare("NewPassword", ErrorMessage = "The new password and confirmation password do not match.")]
	public string ConfirmPassword { get; set; } = "";

	public async Task<IActionResult> OnGet()
	{
		var user = await _signInManager.GetRequiredUser(User);

		var hasPassword = await _signInManager.UserManager.HasPasswordAsync(user);
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

		var user = await _signInManager.GetRequiredUser(User);

		if (!_userManager.IsPasswordAllowed(user.UserName, user.Email, NewPassword))
		{
			ModelState.AddModelError(nameof(NewPassword), "This password is not allowed, please ensure your password is sufficiently different from your username and/or email");
		}

		if (!ModelState.IsValid)
		{
			return Page();
		}

		var changePasswordResult = await _signInManager.UserManager.ChangePasswordAsync(user, OldPassword, NewPassword);
		if (!changePasswordResult.Succeeded)
		{
			AddErrors(changePasswordResult);
			return Page();
		}

		await _signInManager.SignInAsync(user, isPersistent: false);
		var code = await _signInManager.UserManager.GeneratePasswordResetTokenAsync(user);
		var callbackUrl = Url.ResetPasswordCallbackLink(user.Id.ToString(), code, "https");
		await _emailService.PasswordResetConfirmation(user.Email, callbackUrl);
		SuccessStatusMessage("Your password has been changed.");
		return BasePageRedirect("Index");
	}
}
