using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

using TASVideos.Data.Entity;
using TASVideos.Services;
using TASVideos.Services.ExternalMediaPublisher;

namespace TASVideos.Pages.Account
{
	[AllowAnonymous]
	public class ConfirmEmailModel : BasePageModel
	{
		private readonly UserManager _userManager;
		private readonly SignInManager<User> _signInManager;
		private readonly ExternalMediaPublisher _publisher;

		public ConfirmEmailModel(
			UserManager userManager,
			SignInManager<User> signInManager,
			ExternalMediaPublisher publisher)
		{
			_userManager = userManager;
			_signInManager = signInManager;
			_publisher = publisher;
		}

		public async Task<IActionResult> OnGet(string userId, string code)
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

			var result = await _userManager.ConfirmEmailAsync(user, code);
			if (!result.Succeeded)
			{
				return RedirectToPage("/Error");
			}

			await _signInManager.SignInAsync(user, isPersistent: false);
			_publisher.SendUserManagement($"New User joined! {user.UserName}", "", $"{BaseUrl}/Users/Profile/{user.UserName}");
			return Page();
		}
	}
}
