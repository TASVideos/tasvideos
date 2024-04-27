﻿using TASVideos.Core.Services.Email;

namespace TASVideos.Pages.Profile;

[Authorize]
public class ChangeEmailModel(UserManager userManager, ICacheService cache, IEmailService emailService) : BasePageModel
{
	[BindProperty]
	[Display(Name = "Current Email")]
	public string CurrentEmail { get; set; } = "";

	[BindProperty]
	public bool IsEmailConfirmed { get; set; }

	[Required]
	[EmailAddress]
	[BindProperty]
	[Display(Name = "New Email")]
	public string NewEmail { get; set; } = "";

	public async Task OnGet()
	{
		var user = await userManager.GetRequiredUser(User);
		CurrentEmail = user.Email;
		IsEmailConfirmed = user.EmailConfirmed;
	}

	public async Task<IActionResult> OnPost()
	{
		if (!ModelState.IsValid)
		{
			return Page();
		}

		var token = await userManager.GenerateChangeEmailToken(User, NewEmail);

		if (string.IsNullOrWhiteSpace(token))
		{
			return BadRequest("Error generating change email token");
		}

		cache.Set(token, NewEmail);

		var callbackUrl = Url.EmailChangeConfirmationLink(token);
		await emailService.EmailConfirmation(NewEmail, callbackUrl);

		return RedirectToPage("EmailConfirmationSent");
	}
}
