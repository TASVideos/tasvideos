using TASVideos.Core.Services.Email;

namespace TASVideos.Pages.Account;

[BindProperties]
[AllowAnonymous]
[IpBanCheck]
public class EmailConfirmationSentModel : BasePageModel
{
	[StringLength(256)]
	public string UserName { get; set; } = "";

	[EmailAddress]
	public string Email { get; set; } = "";
	public IActionResult OnGet()
	{
		return User.IsLoggedIn()
			? BaseReturnUrlRedirect()
			: Page();
	}

	public async Task<IActionResult> OnPost(
		[FromServices] IUserManager userManager,
		[FromServices] IEmailService emailService,
		[FromServices] ICaptchaService captcha,
		[FromServices] IHostEnvironment env)
	{
		var (isCaptchaValid, captchaFailureReason) = await captcha.VerifyAsync(Request.Form["altcha"]!);
		if (!env.IsDevelopment() && !isCaptchaValid)
		{
			ModelState.AddModelError("", $"TASVideos prefers human users. If you believe you have received this message in error, please contact admin@tasvideos.org. {captcha.ProviderName} says: {captchaFailureReason}");
		}

		var user = await userManager.GetUserByEmailAndUserName(Email, UserName);
		if (user is not null && !user.EmailConfirmed)
		{
			var token = await userManager.GenerateEmailConfirmationToken(user);
			var callbackUrl = Url.EmailConfirmationLink(user.Id.ToString(), token);

			await emailService.EmailConfirmation(Email, callbackUrl);
		}

		SuccessStatusMessage("Email confirmation sent");
		return RedirectToPage("EmailConfirmationSent");
	}
}
