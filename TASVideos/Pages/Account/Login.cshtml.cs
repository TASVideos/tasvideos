using Microsoft.AspNetCore.Authentication;

namespace TASVideos.Pages.Account;

[AllowAnonymous]
[IpBanCheck]
public class LoginModel(SignInManager signInManager, IHostEnvironment env) : BasePageModel
{
	[BindProperty]
	[Display(Name = "User Name")]
	public string UserName { get; set; } = "";

	[BindProperty]
	[DataType(DataType.Password)]
	public string Password { get; set; } = "";

	[BindProperty]
	[Display(Name = "Remember me?")]
	public bool RememberMe { get; set; }

	public async Task<IActionResult> OnGet()
	{
		var user = await signInManager.UserManager.GetUserAsync(User);
		if (user is not null)
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

		var (result, user) = await signInManager.SignIn(UserName, Password, RememberMe);

		if (result.Succeeded)
		{
			return BaseReturnUrlRedirect();
		}

		if (user is not null && !await signInManager.UserManager.IsEmailConfirmedAsync(user) && !env.IsDevelopment())
		{
			return RedirectToPage("/Account/EmailConfirmationSent");
		}

		if (result.IsLockedOut)
		{
			return RedirectToPage("/Account/Lockout");
		}

		if (result.IsNotAllowed)
		{
			return AccessDenied();
		}

		ModelState.AddModelError(string.Empty, "Invalid login attempt.");
		return Page();
	}
}
