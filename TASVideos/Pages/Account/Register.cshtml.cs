using AspNetCore.ReCaptcha;
using TASVideos.Core.Services.Email;

namespace TASVideos.Pages.Account;

[BindProperties]
[AllowAnonymous]
[IpBanCheck]
public class RegisterModel : BasePageModel
{
	[Required]
	public string TimeZone { get; set; } = "";

	[StringLength(50)]
	public string UserName { get; set; } = "";

	[EmailAddress]
	public string Email { get; set; } = "";

	[StringLength(128, ErrorMessage = "The {0} must be at least {2} and at max {1} characters long.", MinimumLength = 12)]
	[DataType(DataType.Password)]
	public string Password { get; set; } = "";

	[StringLength(128, ErrorMessage = "The {0} must be at least {2} and at max {1} characters long.", MinimumLength = 12)]
	[DataType(DataType.Password)]
	public string ConfirmPassword { get; set; } = "";

	[StringLength(256)]
	public string? Location { get; set; }

	[MustBeTrue(ErrorMessage = "You must certify that you are 13 years of age or older")]
	public bool Coppa { get; set; }

	public async Task<IActionResult> OnPost(
		[FromServices] ISignInManager signInManager,
		[FromServices] IUserManager userManager,
		[FromServices] IEmailService emailService,
		[FromServices] IExternalMediaPublisher publisher,
		[FromServices] IReCaptchaService reCaptchaService,
		[FromServices] IHostEnvironment env,
		[FromServices] IUserMaintenanceLogger userMaintenanceLogger)
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
			TimeZoneId = TimeZone,
			From = Location,
			EmailOnPrivateMessage = true
		};

		var result = await userManager.Create(user, Password);
		if (!result.Succeeded)
		{
			AddErrors(result);
			return Page();
		}

		var token = await userManager.GenerateEmailConfirmationToken(user);
		var callbackUrl = Url.EmailConfirmationLink(user.Id.ToString(), token);

		await signInManager.SignIn(user, isPersistent: false);
		await publisher.SendUserManagement($"New User registered! [{user.UserName}]({{0}})", user.UserName);
		await userMaintenanceLogger.Log(user.Id, $"New registration from {IpAddress}");
		await emailService.EmailConfirmation(Email, callbackUrl);

		return userManager.IsConfirmedEmailRequired()
			? RedirectToPage("EmailConfirmationSent")
			: BaseReturnUrlRedirect();
	}
}
