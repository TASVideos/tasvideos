using System.Threading.Tasks;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

using TASVideos.Data.Entity;
using TASVideos.Services;

namespace TASVideos.Pages.Account
{
	[Authorize]
	public class LogoutModel : BasePageModel
	{
		private readonly SignInManager<User> _signInManager;

		public LogoutModel(
			SignInManager<User> signInManager,
			UserManager userManager)
			: base(userManager)
		{
			_signInManager = signInManager;
		}

		public async Task<IActionResult> OnPost()
		{
			await _signInManager.SignOutAsync();
			return Login();
		}
	}
}
