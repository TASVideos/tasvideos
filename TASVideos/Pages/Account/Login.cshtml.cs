using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TASVideos.Core.Services;
using TASVideos.Data;

namespace TASVideos.Pages.Account;

[AllowAnonymous]
[IpBanCheck]
public class LoginModel : BasePageModel
{
	private readonly SignInManager _signInManager;
	private readonly UserManager _userManager;
	private readonly ApplicationDbContext _db;
	private readonly IHostEnvironment _env;

	public LoginModel(
		SignInManager signInManager,
		UserManager userManager,
		ApplicationDbContext db,
		IHostEnvironment env)
	{
		_signInManager = signInManager;
		_userManager = userManager;
		_db = db;
		_env = env;
	}

	[BindProperty]
	[Required]
	[Display(Name = "User Name")]
	public string UserName { get; set; } = "";

	[BindProperty]
	[Required]
	[DataType(DataType.Password)]
	public string Password { get; set; } = "";

	[BindProperty]
	[Display(Name = "Remember me?")]
	public bool RememberMe { get; set; }

	public async Task<IActionResult> OnGet()
	{
		var user = await _signInManager.UserManager.GetUserAsync(User);
		if (user != null)
		{
			return BaseReturnUrlRedirect();
		}

		await HttpContext.SignOutAsync();
		return Page();
	}

	public async Task<IActionResult> OnPost()
	{
		UserName = UserName.Trim();
		if (!ModelState.IsValid)
		{
			return Page();
		}

		var result = await _signInManager.SignIn(UserName, Password, RememberMe);

		if (result.Succeeded)
		{
			return BaseReturnUrlRedirect();
		}

		var user = await _db.Users.SingleOrDefaultAsync(u => u.UserName == UserName);
		if (user is not null && !await _userManager.IsEmailConfirmedAsync(user) && !_env.IsDevelopment())
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
