namespace TASVideos.Pages.Account;

[IpBanCheck]
public class ResetPasswordModel(IUserManager userManager) : BasePageModel
{
	public string Email { get; set; } = "";

	public string UserName { get; set; } = "";

	[BindProperty]
	[StringLength(100, ErrorMessage = "The {0} must be at least {2} and at max {1} characters long.", MinimumLength = 12)]
	[DataType(DataType.Password)]
	public string NewPassword { get; set; } = "";

	[BindProperty]
	[DataType(DataType.Password)]
	[Compare(nameof(NewPassword), ErrorMessage = "The password and confirmation password do not match.")]
	public string ConfirmNewPassword { get; set; } = "";

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

		var user = await userManager.FindById(UserId);
		if (user is null)
		{
			return Home();
		}

		if (!await userManager.VerifyUserToken(user, Code))
		{
			return Home();
		}

		Email = user.Email;
		UserName = user.UserName;
		return Page();
	}

	public async Task<IActionResult> OnPost()
	{
		if (!ModelState.IsValid)
		{
			return Page();
		}

		if (string.IsNullOrWhiteSpace(Code))
		{
			return Home();
		}

		var user = await userManager.FindById(UserId);
		if (user is null)
		{
			return Home();
		}

		var code = Code ?? "";
		var result = await userManager.ResetPasswordAsync(user, code, NewPassword);
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
