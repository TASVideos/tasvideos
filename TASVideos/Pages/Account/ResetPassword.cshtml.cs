using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;

using Microsoft.AspNetCore.Mvc;

using TASVideos.Services;

namespace TASVideos.Pages.Account
{
	public class ResetPasswordModel : BasePageModel
	{
		private readonly UserManager _userManager;

		public ResetPasswordModel(
			UserManager userManager)
		{
			_userManager = userManager;
		}

		[BindProperty]
		[Required]
		[EmailAddress]
		public string Email { get; set; }

		[BindProperty]
		[Required]
		[StringLength(100, ErrorMessage = "The {0} must be at least {2} and at max {1} characters long.", MinimumLength = 12)]
		[DataType(DataType.Password)]
		public string Password { get; set; }

		[BindProperty]
		[DataType(DataType.Password)]
		[Display(Name = "Confirm password")]
		[Compare(nameof(Password), ErrorMessage = "The password and confirmation password do not match.")]
		public string ConfirmPassword { get; set; }

		[FromQuery]
		public string Code { get; set; }

		public IActionResult OnGet()
		{
			if (Code == null)
			{
				return Home();
			}

			return Page();
		}

		public async Task<IActionResult> OnPost()
		{
			if (!ModelState.IsValid)
			{
				return Page();
			}

			var user = await _userManager.FindByEmailAsync(Email);
			if (user == null)
			{
				// Don't reveal that the user does not exist
				return RedirectToPage("ResetPasswordConfirmation");
			}

			var result = await _userManager.ResetPasswordAsync(user, Code, Password);
			if (result.Succeeded)
			{
				return RedirectToPage("ResetPasswordConfirmation");
			}

			AddErrors(result);

			// If we got this far, something failed, redisplay form
			return Page();
		}
	}
}
