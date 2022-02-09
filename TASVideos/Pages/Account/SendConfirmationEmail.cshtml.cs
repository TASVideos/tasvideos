using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TASVideos.Core.Services;
using TASVideos.Core.Services.Email;

namespace TASVideos.Pages.Account;

[Authorize]
[IpBanCheck]
public class SendConfirmationEmail : BasePageModel
{
	private readonly UserManager _userManager;
	private readonly IEmailService _emailService;

	public SendConfirmationEmail(UserManager userManager, IEmailService emailService)
	{
		_userManager = userManager;
		_emailService = emailService;
	}

	public async Task<IActionResult> OnPost()
	{
		var user = await _userManager.GetUserAsync(User);
		var token = await _userManager.GenerateEmailConfirmationTokenAsync(user);
		var callbackUrl = Url.EmailConfirmationLink(user.Id.ToString(), token, Request.Scheme);
		await _emailService.EmailConfirmation(user.Email, callbackUrl);

		return RedirectToPage("EmailConfirmationSent");
	}
}
