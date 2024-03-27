namespace TASVideos.Pages.Profile;

[Authorize]
public class SetPasswordModel(SignInManager signInManager) : BasePageModel
{
	[StringLength(100, ErrorMessage = "The {0} must be at least {2} and at max {1} characters long.", MinimumLength = 12)]
	[DataType(DataType.Password)]
	[Display(Name = "New password")]
	public string NewPassword { get; set; } = "";

	[DataType(DataType.Password)]
	[Display(Name = "Confirm new password")]
	[Compare(nameof(NewPassword), ErrorMessage = "The new password and confirmation password do not match.")]
	public string ConfirmPassword { get; set; } = "";

	public async Task<IActionResult> OnGet()
	{
		var user = await signInManager.UserManager.GetRequiredUser(User);
		var hasPassword = await signInManager.UserManager.HasPasswordAsync(user);

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
		return BasePageRedirect("SetPassword");
	}
}
