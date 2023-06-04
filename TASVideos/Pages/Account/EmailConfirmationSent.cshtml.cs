using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TASVideos.Core.Services;
using TASVideos.Core.Services.Email;
using TASVideos.Data;

namespace TASVideos.Pages.Account;

[AllowAnonymous]
[IpBanCheck]
public class EmailConfirmationSentModel : BasePageModel
{
	private readonly SignInManager _signInManager;
	private readonly ApplicationDbContext _db;
	private readonly IEmailService _emailService;

	public EmailConfirmationSentModel(
		SignInManager signInManager,
		ApplicationDbContext db,
		IEmailService emailService)
	{
		_signInManager = signInManager;
		_db = db;
		_emailService = emailService;
	}

	[BindProperty]
	[Required]
	[StringLength(256)]
	[Display(Name = "User Name")]
	public string UserName { get; set; } = "";

	[BindProperty]
	[Required]
	[EmailAddress]
	[Display(Name = "Email")]
	public string Email { get; set; } = "";
	public async Task<IActionResult> OnPost()
	{
		if (!await _signInManager.EmailAndUserNameMatch(UserName, Email))
		{
			ModelState.AddModelError(nameof(Email), "Username and email entered do not match. Please try again.");
		}

		if (!ModelState.IsValid)
		{
			return Page();
		}

		if (ModelState.IsValid)
		{
			var user = _db.Users.Where(u => u.Email == Email && u.UserName == UserName).FirstOrDefault();
			if (user != null)
			{
				var token = await _signInManager.UserManager.GenerateEmailConfirmationTokenAsync(user);
				var callbackUrl = Url.EmailConfirmationLink(user.Id.ToString(), token, Request.Scheme);

				await _emailService.EmailConfirmation(Email, callbackUrl);

				return _signInManager.UserManager.Options.SignIn.RequireConfirmedEmail
					? RedirectToPage("EmailConfirmationSent")
					: BaseReturnUrlRedirect();
			}
		}

		return Page();
	}
}
