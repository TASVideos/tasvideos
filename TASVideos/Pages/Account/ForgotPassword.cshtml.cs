using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

using TASVideos.Data.Entity;
using TASVideos.Services;

namespace TASVideos.Pages.Account
{
	[AllowAnonymous]
	public class ForgotPasswordModel : PageModel
	{
		private readonly UserManager<User> _userManager;
		private readonly IEmailSender _emailSender;

		public ForgotPasswordModel(UserManager<User> userManager, IEmailSender emailSender)
		{
			_userManager = userManager;
			_emailSender = emailSender;

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
				if (user == null || !(await _userManager.IsEmailConfirmedAsync(user)))
				{
					// Don't reveal that the user does not exist or is not confirmed
					return RedirectToPage("ForgotPasswordConfirmation");
				}

				// For more information on how to enable account confirmation and password reset please
				// visit https://go.microsoft.com/fwlink/?LinkID=532713
				var code = await _userManager.GeneratePasswordResetTokenAsync(user);
				var callbackUrl = Url.ResetPasswordCallbackLink(user.Id.ToString(), code, Request.Scheme);
				await _emailSender.SendEmailAsync(
					Email,
					"Reset Password",
				   $"Please reset your password by clicking here: <a href='{callbackUrl}'>link</a>");
				return RedirectToPage("ForgotPasswordConfirmation");
			}

			// If we got this far, something failed, redisplay form
			return Page();
		}
	}
}
