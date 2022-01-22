using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TASVideos.Core.Services;

namespace TASVideos.Pages.Account
{
	[Authorize]
	[IgnoreAntiforgeryToken]
	public class LogoutModel : BasePageModel
	{
		private readonly SignInManager _signInManager;

		public LogoutModel(SignInManager signInManager)
		{
			_signInManager = signInManager;
		}

		public async Task<IActionResult> OnPost()
		{
			var user = await _signInManager.UserManager.GetUserAsync(User);
			await _signInManager.UserManager.RemoveClaimsAsync(user, User.Claims);
			await _signInManager.SignOutAsync();
			return Login();
		}
	}
}
