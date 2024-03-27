using TASVideos.Core.Services.Email;

namespace TASVideos.Pages.Profile;

[Authorize]
public class ChangeEmailModel(
	UserManager userManager,
	ICacheService cache,
	IEmailService emailService)
	: BasePageModel
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
	public string? NewEmail { get; set; }

	public async Task<IActionResult> OnGet()
	{
		var user = await userManager.GetUserAsync(User);
		if (user is null)
		{
			return AccessDenied();
		}

		CurrentEmail = user.Email;
		IsEmailConfirmed = user.EmailConfirmed;

		return Page();
	}

	public async Task<IActionResult> OnPost()
	{
		if (!ModelState.IsValid)
		{
			return Page();
		}

		var user = await userManager.GetUserAsync(User);
		if (user is null)
		{
			return AccessDenied();
		}

		var token = await userManager.GenerateChangeEmailTokenAsync(user, NewEmail!);

		if (string.IsNullOrWhiteSpace(token))
		{
			return BadRequest("Error generating change email token");
		}

		cache.Set(token, NewEmail);

		var callbackUrl = Url.EmailChangeConfirmationLink(token, Request.Scheme);
		await emailService.EmailConfirmation(NewEmail!, callbackUrl);

		return RedirectToPage("EmailConfirmationSent");
	}
}
