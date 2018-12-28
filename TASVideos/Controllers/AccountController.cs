using System.Threading.Tasks;

using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

using TASVideos.Data.Entity;
using TASVideos.Models;
using TASVideos.Tasks;

namespace TASVideos.Controllers
{
	[Authorize]
	[Route("[controller]/[action]")]
	public class AccountController : BaseController
	{
		private readonly SignInManager<User> _signInManager;
		private readonly ILogger _logger;

		public AccountController(
			SignInManager<User> signInManager,
			ILogger<AccountController> logger,
			UserTasks userTasks)
			: base(userTasks)
		{
			_signInManager = signInManager;
			_logger = logger;
		}

		[AllowAnonymous]
		public IActionResult Index()
		{
			return RedirectToAction("Login");
		}

		[HttpGet]
		[AllowAnonymous]
		public async Task<IActionResult> Login(string returnUrl = null)
		{
			// Clear the existing external cookie to ensure a clean login process
			await HttpContext.SignOutAsync(IdentityConstants.ExternalScheme);

			ViewData["ReturnUrl"] = returnUrl;
			return View();
		}

		[AllowAnonymous]
		[HttpPost, ValidateAntiForgeryToken]
		public async Task<IActionResult> Login(LoginModel model, string returnUrl = null)
		{
			ViewData["ReturnUrl"] = returnUrl;
			if (ModelState.IsValid)
			{
				var result = await UserTasks.PasswordSignIn(model);

				if (result.Succeeded)
				{
					_logger.LogInformation("User logged in.");
					await UserTasks.MarkUserLoggedIn(model.UserName);
					return RedirectToLocal(returnUrl);
				}

				if (result.IsLockedOut)
				{
					_logger.LogWarning("User account locked out.");
					return RedirectToPage("/Account/Lockout");
				}

				ModelState.AddModelError(string.Empty, "Invalid login attempt.");
				return View(model);
			}

			// If we got this far, something failed, redisplay form
			return View(model);
		}

		[HttpPost, ValidateAntiForgeryToken]
		public async Task<IActionResult> Logout()
		{
			await _signInManager.SignOutAsync();
			_logger.LogInformation("User logged out.");
			return RedirectToLogin();
		}
	}
}
