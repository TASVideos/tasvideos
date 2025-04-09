using Microsoft.AspNetCore.Authentication;

namespace TASVideos.Pages.Account;

[BindProperties]
[AllowAnonymous]
[IpBanCheck]
public class LoginModel(ISignInManager signInManager, IUserManager userManager, IHostEnvironment env) : BasePageModel
{
	public string UserName { get; set; } = "";

	[DataType(DataType.Password)]
	public string Password { get; set; } = "";
	public bool RememberMe { get; set; }

	public async Task<IActionResult> OnGet()
	{
		if (User.Identity?.IsAuthenticated ?? false)
		{
			return BaseReturnUrlRedirect();
		}

		await HttpContext.SignOutAsync();
		return Page();
	}

	public async Task<IActionResult> OnPost()
	{
		if (!ModelState.IsValid)
		{
			return Page();
		}

		var (result, user, failedDueToBan) = await signInManager.SignIn(UserName, Password, RememberMe);

		if (result.Succeeded)
		{
			return BaseReturnUrlRedirect();
		}

		if (user is not null && failedDueToBan)
		{
			TempData[nameof(BannedModel.BannedUserId)] = user.Id;
			return RedirectToPage("/Account/Banned");
		}

		if (user is not null && !await userManager.IsEmailConfirmed(user) && !env.IsDevelopment())
		{
			return RedirectToPage("/Account/EmailConfirmationSent");
		}

		if (result.IsLockedOut)
		{
			return RedirectToPage("/Account/Lockout");
		}

		ModelState.AddModelError("", "Invalid login attempt.");
		return Page();
	}
}
