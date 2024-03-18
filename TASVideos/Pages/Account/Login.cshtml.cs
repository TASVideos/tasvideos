﻿using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TASVideos.Core.Services;
using TASVideos.Data;

namespace TASVideos.Pages.Account;

[AllowAnonymous]
[IpBanCheck]
public class LoginModel(
	SignInManager signInManager,
	ApplicationDbContext db,
	IHostEnvironment env)
	: BasePageModel
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

		UserName = UserName.Trim().Replace(" ", "_");

		var result = await signInManager.SignIn(UserName, Password, RememberMe);

		if (result.Succeeded)
		{
			return BaseReturnUrlRedirect();
		}

		var user = await db.Users.SingleOrDefaultAsync(u => u.UserName == UserName);
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
