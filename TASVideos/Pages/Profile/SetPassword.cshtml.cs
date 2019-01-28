using System;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

using TASVideos.Data.Entity;
using TASVideos.Extensions;
using TASVideos.Services;

namespace TASVideos.Pages.Profile
{
	[Authorize]
	public class SetPasswordModel : BasePageModel
	{
		private readonly UserManager _userManager;
		private readonly SignInManager<User> _signInManager;

		public SetPasswordModel(
			UserManager userManager,
			SignInManager<User> signInManager)
		{
			_userManager = userManager;
			_signInManager = signInManager;
		}

		[TempData]
		public string StatusMessage { get; set; }

		[Required]
		[StringLength(100, ErrorMessage = "The {0} must be at least {2} and at max {1} characters long.", MinimumLength = 12)]
		[DataType(DataType.Password)]
		[Display(Name = "New password")]
		public string NewPassword { get; set; }

		[DataType(DataType.Password)]
		[Display(Name = "Confirm new password")]
		[Compare(nameof(NewPassword), ErrorMessage = "The new password and confirmation password do not match.")]
		public string ConfirmPassword { get; set; }

		public async Task<IActionResult> OnGet()
		{
			var user = await _userManager.GetUserAsync(User);
			if (user == null)
			{
				throw new ApplicationException($"Unable to load user with ID '{User.GetUserId()}'.");
			}

			var hasPassword = await _userManager.HasPasswordAsync(user);

			if (hasPassword)
			{
				return RedirectToPage("ChangePassword");
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

			var addPasswordResult = await _userManager.AddPasswordAsync(user, NewPassword);
			if (!addPasswordResult.Succeeded)
			{
				AddErrors(addPasswordResult);
				return Page();
			}

			await _signInManager.SignInAsync(user, isPersistent: false);
			StatusMessage = "Your password has been set.";

			return RedirectToPage("SetPassword");
		}
	}
}
