namespace TASVideos.Pages.Profile;

[Authorize]
public class SetPasswordModel(ISignInManager signInManager) : BasePageModel
{
	[StringLength(100, ErrorMessage = "The {0} must be at least {2} and at max {1} characters long.", MinimumLength = 12)]
	[DataType(DataType.Password)]
	public string NewPassword { get; set; } = "";

	[DataType(DataType.Password)]
	[Compare(nameof(NewPassword), ErrorMessage = "The new password and confirmation password do not match.")]
	public string ConfirmNewPassword { get; set; } = "";

	public async Task<IActionResult> OnGet()
	{
		var hasPassword = await signInManager.HasPassword(User);
		if (hasPassword)
		{
			return RedirectToPage("ChangePassword");
		}

		return Page();
	}

	public async Task<IActionResult> OnPost()
	{
		if (!ModelState.IsValid)
		{
			return Page();
		}

		var result = await signInManager.AddPassword(User, NewPassword);
		if (!result.Succeeded)
		{
			AddErrors(result);
			return Page();
		}

		SuccessStatusMessage("Your password has been set.");
		return BasePageRedirect("ChangePassword");
	}
}
