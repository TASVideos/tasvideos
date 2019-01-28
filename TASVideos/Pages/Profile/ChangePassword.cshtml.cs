using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

using TASVideos.Data.Entity;
using TASVideos.Services;

namespace TASVideos.Pages.Profile
{
	[Authorize]
	public class ChangePasswordModel : BasePageModel
	{
		private readonly UserManager _userManager;
		private readonly SignInManager<User> _signInManager;

		public ChangePasswordModel(
			SignInManager<User> signInManager,
			UserManager userManager)
		{
			_userManager = userManager;
			_signInManager = signInManager;
		}

		[TempData]
		public string StatusMessage { get; set; }

		[BindProperty]
		[Required]
		[DataType(DataType.Password)]
		[Display(Name = "Current password")]
		public string OldPassword { get; set; }

		[BindProperty]
		[Required]
		[StringLength(100, ErrorMessage = "The {0} must be at least {2} and at max {1} characters long.", MinimumLength = 12)]
		[DataType(DataType.Password)]
		[Display(Name = "New password")]
		public string NewPassword { get; set; }

		[BindProperty]
		[DataType(DataType.Password)]
		[Display(Name = "Confirm new password")]
		[Compare("NewPassword", ErrorMessage = "The new password and confirmation password do not match.")]
		public string ConfirmPassword { get; set; }
		
		public async Task<IActionResult> OnGet()
		{
			var user = await _userManager.GetUserAsync(User);

			var hasPassword = await _userManager.HasPasswordAsync(user);
			if (!hasPassword)
			{
				return RedirectToPage("SetPassword");
			}

			return Page();
		}

		public async Task<IActionResult> OnPost()
		{
			if (!ModelState.IsValid)
			{
				return Page();
			}

			var user = await _userManager.GetUserAsync(User);

			var changePasswordResult = await _userManager.ChangePasswordAsync(user, OldPassword, NewPassword);
			if (!changePasswordResult.Succeeded)
			{
				AddErrors(changePasswordResult);
				return Page();
			}

			await _signInManager.SignInAsync(user, isPersistent: false);
			StatusMessage = "Your password has been changed.";

			return RedirectToPage("ChangePassword");
		}
	}
}
