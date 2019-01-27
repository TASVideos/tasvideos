using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TASVideos.Services;

namespace TASVideos.Pages.Account
{
	[AllowAnonymous]
	public class ConfirmEmailModel : BasePageModel
	{
		private readonly UserManager _userManager;

		public ConfirmEmailModel(
			UserManager userManager)
			: base(userManager)
		{
			_userManager = userManager;
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

			return Page();
		}
	}
}
