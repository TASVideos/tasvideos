using TASVideos.Core.Services.Email;

namespace TASVideos.Pages.Account;

[Authorize]
[IpBanCheck]
public class SendConfirmationEmail(IUserManager userManager, IEmailService emailService) : BasePageModel
{
	public async Task<IActionResult> OnPost()
	{
		var user = await userManager.GetRequiredUser(User);
		var token = await userManager.GenerateEmailConfirmationToken(user);
		var callbackUrl = Url.EmailConfirmationLink(user.Id.ToString(), token);
		await emailService.EmailConfirmation(user.Email, callbackUrl);

		return RedirectToPage("EmailConfirmationSent");
	}
}
