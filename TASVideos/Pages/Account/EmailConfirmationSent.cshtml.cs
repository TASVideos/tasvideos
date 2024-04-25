using TASVideos.Core.Services.Email;

namespace TASVideos.Pages.Account;

[AllowAnonymous]
[IpBanCheck]
public class EmailConfirmationSentModel(SignInManager signInManager, IEmailService emailService) : BasePageModel
{
	[BindProperty]
	[StringLength(256)]
	[Display(Name = "User Name")]
	public string UserName { get; set; } = "";

	[BindProperty]
	[EmailAddress]
	public string Email { get; set; } = "";
	public IActionResult OnGet()
	{
		return User.IsLoggedIn()
			? BaseReturnUrlRedirect()
			: Page();
	}

	public async Task<IActionResult> OnPost()
	{
		var user = await signInManager.GetUserByEmailAndUserName(Email, UserName);
		if (user is not null && !user.EmailConfirmed)
		{
			var token = await signInManager.UserManager.GenerateEmailConfirmationTokenAsync(user);
			var callbackUrl = Url.EmailConfirmationLink(user.Id.ToString(), token);

			await emailService.EmailConfirmation(Email, callbackUrl);
		}

		SuccessStatusMessage("Email confirmation sent");
		return RedirectToPage("EmailConfirmationSent");
	}
}
