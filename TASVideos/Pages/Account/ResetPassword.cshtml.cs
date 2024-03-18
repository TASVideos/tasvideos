using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;
using TASVideos.Core.Services;

namespace TASVideos.Pages.Account;

[IpBanCheck]
public class ResetPasswordModel(UserManager userManager) : BasePageModel
{
	[BindProperty]
	[EmailAddress]
	public string Email { get; set; } = "";

	[BindProperty]
	[Display(Name = "New password")]
	[StringLength(100, ErrorMessage = "The {0} must be at least {2} and at max {1} characters long.", MinimumLength = 12)]
	[DataType(DataType.Password)]
	public string Password { get; set; } = "";

	[BindProperty]
	[DataType(DataType.Password)]
	[Display(Name = "Confirm new password")]
	[Compare(nameof(Password), ErrorMessage = "The password and confirmation password do not match.")]
	public string ConfirmPassword { get; set; } = "";

	[FromQuery]
	public string? Code { get; set; }

	[FromQuery]
	public string? UserId { get; set; }

	public async Task<IActionResult> OnGet()
	{
		if (string.IsNullOrWhiteSpace(Code))
		{
			return Home();
		}

		var user = await userManager.FindByIdAsync(UserId ?? "");
		if (user is null)
		{
			return Home();
		}

		Email = user.Email;
		return Page();
	}

	public async Task<IActionResult> OnPost()
	{
		if (!ModelState.IsValid)
		{
			return Page();
		}

		var user = await userManager.FindByEmailAsync(Email);
		if (user is null)
		{
			// Don't reveal that the user does not exist
			return RedirectToPage("ResetPasswordConfirmation");
		}

		var code = Code ?? "";
		var result = await userManager.ResetPasswordAsync(user, code, Password);
		if (result.Succeeded)
		{
			await userManager.MarkEmailConfirmed(user);
			return RedirectToPage("ResetPasswordConfirmation");
		}

		AddErrors(result);

		// If we got this far, something failed, redisplay form
		return Page();
	}
}
