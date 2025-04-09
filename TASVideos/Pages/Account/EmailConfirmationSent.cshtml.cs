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
		[FromServices] IUserManager userManager, [FromServices] IEmailService emailService)
	{
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
