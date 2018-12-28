using System.Threading.Tasks;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

using TASVideos.Data.Entity;
using TASVideos.Tasks;

namespace TASVideos.Controllers
{
	[Authorize]
	[Route("[controller]/[action]")]
	public class AccountController : BaseController
	{
		private readonly SignInManager<User> _signInManager;

		public AccountController(
			SignInManager<User> signInManager,
			UserTasks userTasks)
			: base(userTasks)
		{
			_signInManager = signInManager;
		}

		[HttpPost, ValidateAntiForgeryToken]
		public async Task<IActionResult> Logout()
		{
			await _signInManager.SignOutAsync();
			return ReRouteToLogin();
		}
	}
}
