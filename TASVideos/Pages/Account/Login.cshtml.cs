﻿using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TASVideos.Core.Services;

namespace TASVideos.Pages.Account;

[AllowAnonymous]
[IpBanCheck]
public class LoginModel : BasePageModel
{
	private readonly SignInManager _signInManager;

	public LoginModel(SignInManager signInManager)
	{
		_signInManager = signInManager;
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
		if (!ModelState.IsValid)
		{
			return Page();
		}

		var result = await _signInManager.SignIn(UserName, Password, RememberMe);

		if (result.Succeeded)
		{
			return BaseReturnUrlRedirect();
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
