using System.Threading.Tasks;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

using TASVideos.Data.Entity;

namespace TASVideos.Pages.Account
{
	[Authorize]
	public class LogoutModel : BasePageModel
	{
		private readonly SignInManager<User> _signInManager;

		public LogoutModel(SignInManager<User> signInManager)
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
