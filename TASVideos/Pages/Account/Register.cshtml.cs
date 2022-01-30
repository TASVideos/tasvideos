using System.ComponentModel.DataAnnotations;
using System.Text.RegularExpressions;
using AspNetCore.ReCaptcha;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TASVideos.Core.Services;
using TASVideos.Core.Services.Email;
using TASVideos.Core.Services.ExternalMediaPublisher;
using TASVideos.Data;
using TASVideos.Data.Entity;
using TASVideos.Models.ValidationAttributes;

namespace TASVideos.Pages.Account;

[AllowAnonymous]
[IpBanCheck]
public class RegisterModel : BasePageModel
{
	private readonly ApplicationDbContext _db;
	private readonly SignInManager _signInManager;
	private readonly IEmailService _emailService;
	private readonly ExternalMediaPublisher _publisher;
	private readonly IReCaptchaService _reCaptchaService;
	private readonly IHostEnvironment _env;
	private readonly IUserMaintenanceLogger _userMaintenanceLogger;

	public RegisterModel(
		ApplicationDbContext db,
		SignInManager signInManager,
		IEmailService emailService,
		ExternalMediaPublisher publisher,
		IReCaptchaService reCaptchaService,
		IHostEnvironment env,
		IUserMaintenanceLogger userMaintenanceLogger)
	{
		_db = db;
		_signInManager = signInManager;
		_emailService = emailService;
		_publisher = publisher;
		_reCaptchaService = reCaptchaService;
		_env = env;
		_userMaintenanceLogger = userMaintenanceLogger;
	}

	[BindProperty]
	[Display(Name = "Time Zone")]
	public string? SelectedTimeZone { get; set; }

	[BindProperty]
	[Required]
	[StringLength(256)]
	[Display(Name = "User Name")]
	public string UserName { get; set; } = "";

	[RegularExpression(@"[^+]+", ErrorMessage = "Email pattern not allowed.")]
	[BindProperty]
	[Required]
	[EmailAddress]
	[Display(Name = "Email")]
	public string Email { get; set; } = "";

	[BindProperty]
	[Required]
	[StringLength(100, ErrorMessage = "The {0} must be at least {2} and at max {1} characters long.", MinimumLength = 12)]
	[DataType(DataType.Password)]
	[Display(Name = "Password")]
	public string Password { get; set; } = "";

	[BindProperty]
	[DataType(DataType.Password)]
	[Display(Name = "Confirm password")]
	public string ConfirmPassword { get; set; } = "";

	[BindProperty]
	[Display(Name = "Location")]
	[StringLength(256)]
	public string? From { get; set; }

	[BindProperty]
	[Required]
	[MustBeTrue(ErrorMessage = "You must certify that you are 13 years of age or older")]
	[Display(Name = "By checking the box below, you certify you are 13 years of age or older")]
	public bool Coppa { get; set; }

	public async Task<IActionResult> OnPost()
	{
		if (Password != ConfirmPassword)
		{
			ModelState.AddModelError(nameof(ConfirmPassword), "The password and confirmation password do not match.");
		}

		string encodedResponse = Request.Form["g-recaptcha-response"];
		bool isCaptchaValid = await _reCaptchaService.VerifyAsync(encodedResponse);

		if (!_env.IsDevelopment() && !isCaptchaValid)
		{
			ModelState.AddModelError("", "TASVideos prefers human users.  If you believe you have received this message in error, please contact admin@tasvideos.org");
		}

		if (!ModelState.IsValid)
		{
			return Page();
		}

		var disallows = await _db.UserDisallows.ToListAsync();
		foreach (var disallow in disallows)
		{
			var regex = new Regex(disallow.RegexPattern);
			if (regex.IsMatch(UserName))
			{
				ModelState.AddModelError(nameof(UserName), "The username is not allowed.");
				break;
			}
		}

		if (ModelState.IsValid)
		{
			var user = new User
			{
				UserName = UserName,
				Email = Email,
				TimeZoneId = SelectedTimeZone ?? TimeZoneInfo.Utc.Id,
				From = From
			};
			var result = await _signInManager.UserManager.CreateAsync(user, Password);
			if (result.Succeeded)
			{
				var token = await _signInManager.UserManager.GenerateEmailConfirmationTokenAsync(user);
				var callbackUrl = Url.EmailConfirmationLink(user.Id.ToString(), token, Request.Scheme);

				await _signInManager.SignInAsync(user, isPersistent: false);
				await _publisher.SendUserManagement(
					$"New User registered! {user.UserName}",
					"",
					$"Users/Profile/{user.UserName}");
				await _userMaintenanceLogger.Log(user.Id, $"New registration from {IpAddress}");
				await _emailService.EmailConfirmation(Email, callbackUrl);

				return _signInManager.UserManager.Options.SignIn.RequireConfirmedEmail
					? RedirectToPage("EmailConfirmationSent")
					: BaseReturnUrlRedirect();
			}

			AddErrors(result);
		}

		return Page();
	}
}
