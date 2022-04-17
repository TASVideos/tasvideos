using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TASVideos.Core.Services;
using TASVideos.Core.Services.Email;

namespace TASVideos.Pages.Profile;

[Authorize]
public class ChangeEmailModel : BasePageModel
{
	private readonly UserManager _userManager;
	private readonly ICacheService _cache;
	private readonly IEmailService _emailService;

	public ChangeEmailModel(
		UserManager userManager,
		ICacheService cache,
		IEmailService emailService)
	{
		_userManager = userManager;
		_cache = cache;
		_emailService = emailService;
	}

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
		var user = await _userManager.GetUserAsync(User);
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

		var user = await _userManager.GetUserAsync(User);
		if (user is null)
		{
			return AccessDenied();
		}

		var token = await _userManager.GenerateChangeEmailTokenAsync(user, NewEmail);

		if (string.IsNullOrWhiteSpace(token))
		{
			return BadRequest("Error generating change email token");
		}

		_cache.Set(token, NewEmail);

		var callbackUrl = Url.EmailChangeConfirmationLink(user.Id.ToString(), token, Request.Scheme);
		await _emailService.EmailConfirmation(NewEmail!, callbackUrl);

		return RedirectToPage("EmailConfirmationSent");
	}
}