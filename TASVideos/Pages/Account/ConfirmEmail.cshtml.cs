﻿using System.Net;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TASVideos.Core.Services;
using TASVideos.Core.Services.ExternalMediaPublisher;

namespace TASVideos.Pages.Account
{
	[AllowAnonymous]
	public class ConfirmEmailModel : BasePageModel
	{
		private readonly UserManager _userManager;
		private readonly SignInManager _signInManager;
		private readonly ExternalMediaPublisher _publisher;

		public ConfirmEmailModel(
			UserManager userManager,
			SignInManager signInManager,
			ExternalMediaPublisher publisher)
		{
			_userManager = userManager;
			_signInManager = signInManager;
			_publisher = publisher;
		}

		public async Task<IActionResult> OnGet(string? userId, string? code)
		{
			if (userId == null || code == null)
			{
				return Home();
			}

			var user = await _userManager.FindByIdAsync(userId);
			if (user == null)
			{
				return Home();
			}

			var result = await _userManager.ConfirmEmailAsync(user, WebUtility.UrlDecode(code));
			if (!result.Succeeded)
			{
				return RedirectToPage("/Error");
			}

			await _signInManager.SignInAsync(user, isPersistent: false);
			_publisher.SendUserManagement($"New User joined! {user.UserName}", "", $"Users/Profile/{user.UserName}", user.UserName);
			return Page();
		}
	}
}
