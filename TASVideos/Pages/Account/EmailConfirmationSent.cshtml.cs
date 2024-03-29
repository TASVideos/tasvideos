﻿using TASVideos.Core.Services.Email;

namespace TASVideos.Pages.Account;

[AllowAnonymous]
[IpBanCheck]
public class EmailConfirmationSentModel(
	SignInManager signInManager,
	ApplicationDbContext db,
	IEmailService emailService)
	: BasePageModel
{
	[BindProperty]
	[StringLength(256)]
	[Display(Name = "User Name")]
	public string UserName { get; set; } = "";

	[BindProperty]
	[EmailAddress]
	[Display(Name = "Email")]
	public string Email { get; set; } = "";
	public IActionResult OnGet()
	{
		return User.IsLoggedIn()
			? BaseReturnUrlRedirect()
			: Page();
	}

	public async Task<IActionResult> OnPost()
	{
		if (await signInManager.EmailAndUserNameMatch(UserName, Email))
		{
			var user = db.Users.SingleOrDefault(u => u.Email == Email && u.UserName == UserName);
			if (user is not null && !user.EmailConfirmed)
			{
				var token = await signInManager.UserManager.GenerateEmailConfirmationTokenAsync(user);
				var callbackUrl = Url.EmailConfirmationLink(user.Id.ToString(), token, Request.Scheme);

				await emailService.EmailConfirmation(Email, callbackUrl);
			}
		}

		SuccessStatusMessage("Email resend received");
		return Page();
	}
}
