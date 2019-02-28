using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

using TASVideos.Services;
using TASVideos.Services.Email;

namespace TASVideos.Pages.Account
{
	[AllowAnonymous]
	public class ForgotPasswordModel : PageModel
	{
		private readonly UserManager _userManager;
		private readonly IEmailService _emailService;

		public ForgotPasswordModel(UserManager userManager, IEmailService emailService)
		{
			_userManager = userManager;
			_emailService = emailService;
		}

		[BindProperty]
		[Required]
		[EmailAddress]
		public string Email { get; set; }

		public async Task<IActionResult> OnPost()
		{
			if (ModelState.IsValid)
			{
				var user = await _userManager.FindByEmailAsync(Email);
				if (user == null || !await _userManager.IsEmailConfirmedAsync(user))
				{
					// Don't reveal that the user does not exist or is not confirmed
					return RedirectToPage("ForgotPasswordConfirmation");
				}

				var code = await _userManager.GeneratePasswordResetTokenAsync(user);
				var callbackUrl = Url.ResetPasswordCallbackLink(user.Id.ToString(), code, Request.Scheme);
				await _emailService.ResetPassword(Email, callbackUrl);

				return RedirectToPage("ForgotPasswordConfirmation");
			}

			// If we got this far, something failed, redisplay form
			return Page();
		}
	}
}
