using AspNetCore.ReCaptcha;
using TASVideos.Core.Services.Email;
using TASVideos.Core.Services.ExternalMediaPublisher;

namespace TASVideos.Pages.Account;

[AllowAnonymous]
[IpBanCheck]
public class RegisterModel(
	SignInManager signInManager,
	IEmailService emailService,
	ExternalMediaPublisher publisher,
	IReCaptchaService reCaptchaService,
	IHostEnvironment env,
	IUserMaintenanceLogger userMaintenanceLogger)
	: BasePageModel
{
	[Required]
	[BindProperty]
	[Display(Name = "Time Zone")]
	public string SelectedTimeZone { get; set; } = "";

	[BindProperty]
	[StringLength(50)]
	[Display(Name = "User Name")]
	public string UserName { get; set; } = "";

	[BindProperty]
	[EmailAddress]
	[Display(Name = "Email")]
	public string Email { get; set; } = "";

	[BindProperty]
	[StringLength(128, ErrorMessage = "The {0} must be at least {2} and at max {1} characters long.", MinimumLength = 12)]
	[DataType(DataType.Password)]
	[Display(Name = "Password")]
	public string Password { get; set; } = "";

	[BindProperty]
	[StringLength(128, ErrorMessage = "The {0} must be at least {2} and at max {1} characters long.", MinimumLength = 12)]
	[DataType(DataType.Password)]
	[Display(Name = "Confirm password")]
	public string ConfirmPassword { get; set; } = "";

	[BindProperty]
	[Display(Name = "Location")]
	[StringLength(256)]
	public string? From { get; set; }

	[BindProperty]
	[MustBeTrue(ErrorMessage = "You must certify that you are 13 years of age or older")]
	[Display(Name = "By checking the box below, you certify you are 13 years of age or older")]
	public bool Coppa { get; set; }

	public async Task<IActionResult> OnPost()
	{
		if (Password != ConfirmPassword)
		{
			ModelState.AddModelError(nameof(ConfirmPassword), "The password and confirmation password do not match.");
		}

		var encodedResponse = Request.Form["g-recaptcha-response"];
		bool isCaptchaValid = await reCaptchaService.VerifyAsync(encodedResponse);

		if (!env.IsDevelopment() && !isCaptchaValid)
		{
			ModelState.AddModelError("", "TASVideos prefers human users.  If you believe you have received this message in error, please contact admin@tasvideos.org");
		}

		if (!ModelState.IsValid)
		{
			return Page();
		}

		if (!await signInManager.UsernameIsAllowed(UserName))
		{
			ModelState.AddModelError(nameof(UserName), "The username is not allowed.");
		}

		if (await signInManager.EmailExists(Email))
		{
			ModelState.AddModelError(nameof(Email), "Email is already taken.");
		}

		if (!signInManager.IsPasswordAllowed(UserName, Email, Password))
		{
			ModelState.AddModelError(nameof(Password), "This password is not allowed, please ensure your password is sufficiently different from your username and/or email");
		}

		if (!ModelState.IsValid)
		{
			return Page();
		}

		var user = new User
		{
			UserName = UserName,
			Email = Email,
			TimeZoneId = SelectedTimeZone,
			From = From,
			EmailOnPrivateMessage = true
		};

		var result = await signInManager.UserManager.CreateAsync(user, Password);
		if (!result.Succeeded)
		{
			AddErrors(result);
			return Page();
		}

		var token = await signInManager.UserManager.GenerateEmailConfirmationTokenAsync(user);
		var callbackUrl = Url.EmailConfirmationLink(user.Id.ToString(), token, Request.Scheme);

		await signInManager.SignInAsync(user, isPersistent: false);
		await publisher.SendUserManagement(
			$"New User registered! {user.UserName}",
			$"New User registered! [{user.UserName}]({{0}})",
			"",
			$"Users/Profile/{Uri.EscapeDataString(user.UserName)}");
		await userMaintenanceLogger.Log(user.Id, $"New registration from {IpAddress}");
		await emailService.EmailConfirmation(Email, callbackUrl);

		return signInManager.UserManager.Options.SignIn.RequireConfirmedEmail
			? RedirectToPage("EmailConfirmationSent")
			: BaseReturnUrlRedirect();
	}
}
