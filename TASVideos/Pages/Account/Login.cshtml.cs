using System;
using System.ComponentModel.DataAnnotations;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

using TASVideos.Data;
using TASVideos.Data.Entity;
using TASVideos.Tasks;

namespace TASVideos.Pages.Account
{
	[AllowAnonymous]
	public class LoginModel : BasePageModel
	{
		private readonly UserManager<User> _userManager;
		private readonly SignInManager<User> _signInManager;

		private readonly ApplicationDbContext _db;

		public LoginModel(
			UserManager<User> userManager,
			SignInManager<User> signInManager,
			ApplicationDbContext db,
			UserTasks userTasks)
			: base(userTasks)
		{
			_userManager = userManager;
			_signInManager = signInManager;
			_db = db;
		}

		[FromQuery]
		public string ReturnUrl { get; set; }

		[BindProperty]
		[Required]
		[Display(Name = "User Name")]
		public string UserName { get; set; }

		[BindProperty]
		[Required]
		[DataType(DataType.Password)]
		public string Password { get; set; }

		[BindProperty]
		[Display(Name = "Remember me?")]
		public bool RememberMe { get; set; }

		public async Task OnGet()
		{
			// Clear the existing external cookie to ensure a clean login process
			await HttpContext.SignOutAsync(IdentityConstants.ExternalScheme);
		}

		public async Task<IActionResult> OnPost()
		{
			if (ModelState.IsValid)
			{
				var result = await PasswordSignIn();

				if (result.Succeeded)
				{
					var user = await _db.Users.SingleAsync(u => u.UserName == UserName);
					user.LastLoggedInTimeStamp = DateTime.UtcNow;
					await _db.SaveChangesAsync();

					return RedirectToLocal(ReturnUrl);
				}

				if (result.IsLockedOut)
				{
					return RedirectToPage("/Account/Lockout");
				}

				ModelState.AddModelError(string.Empty, "Invalid login attempt.");
			}

			// If we got this far, something failed, redisplay form
			return Page();
		}

		private async Task<Microsoft.AspNetCore.Identity.SignInResult> PasswordSignIn() // Using the razor page model, eww?
		{
			var user = await _db.Users.SingleOrDefaultAsync(u => u.UserName == UserName);
			if (user == null)
			{
				return Microsoft.AspNetCore.Identity.SignInResult.Failed;
			}

			// If no password, then try to log in with legacy method
			if (!string.IsNullOrWhiteSpace(user.LegacyPassword))
			{
				using (var md5 = MD5.Create())
				{
					var md5Result = md5.ComputeHash(Encoding.ASCII.GetBytes(Password));
					string encrypted = BitConverter.ToString(md5Result)
						.Replace("-", "")
						.ToLower();

					if (encrypted == user.LegacyPassword)
					{
						user.PasswordHash = _userManager.PasswordHasher.HashPassword(user, Password);
						await _userManager.UpdateSecurityStampAsync(user);
						user.LegacyPassword = null;
						await _db.SaveChangesAsync();
					}
				}
			}

			// This doesn't count login failures towards account lockout
			// To enable password failures to trigger account lockout, set lockoutOnFailure: true
			return await _signInManager.PasswordSignInAsync(
				UserName,
				Password,
				RememberMe,
				lockoutOnFailure: false);
		}
	}
}
