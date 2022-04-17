using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TASVideos.Core.Services;

namespace TASVideos.Pages.Profile;

[Authorize]
public class ChangeEmailModel : BasePageModel
{
	private readonly UserManager _userManager;

	public ChangeEmailModel(UserManager userManager)
	{
		_userManager = userManager;
	}

	[BindProperty]
	[Display(Name = "Current Email")]
	public string CurrentEmail { get; set; } = "";

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

		await _userManager.GenerateChangeEmailTokenAsync(user, NewEmail);

		// TODO:
		return Home();
	}
}