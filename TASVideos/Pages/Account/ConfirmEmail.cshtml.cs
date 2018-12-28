using System.Threading.Tasks;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

using TASVideos.Data.Entity;
using TASVideos.Tasks;

namespace TASVideos.Pages.Account
{
	[AllowAnonymous]
	public class ConfirmEmailModel : BasePageModel
	{
		private readonly UserManager<User> _userManager;

		public ConfirmEmailModel(
			UserManager<User> userManager,
			UserTasks userTasks)
			: base(userTasks)
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
