using AspNetCore.ReCaptcha;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.Extensions.Hosting;
using TASVideos.Core.Services;
using TASVideos.Core.Services.Email;
using TASVideos.Core.Services.ExternalMediaPublisher;
using TASVideos.Pages.Account;
using TASVideos.Services;

namespace TASVideos.RazorPages.Tests.Pages.Account;

[TestClass]
public class RegisterTests : BasePageModelTests
{
	private readonly ISignInManager _signInManager = Substitute.For<ISignInManager>();
	private readonly IUserManager _userManager = Substitute.For<IUserManager>();
	private readonly IEmailService _emailService = Substitute.For<IEmailService>();
	private readonly IExternalMediaPublisher _publisher = Substitute.For<IExternalMediaPublisher>();
	private readonly IReCaptchaService _reCaptchaService = Substitute.For<IReCaptchaService>();
	private readonly IHostEnvironment _env = Substitute.For<IHostEnvironment>();
	private readonly IUserMaintenanceLogger _userMaintenanceLogger = Substitute.For<IUserMaintenanceLogger>();

	private readonly RegisterModel _model = new();

	public RegisterTests()
	{
		var pageContext = TestPageContext();
		pageContext.HttpContext.Request.ContentType = "application/x-www-form-urlencoded";

		_model.PageContext = pageContext;
		_model.TempData = Substitute.For<ITempDataDictionary>();
		_model.Url = GeMockUrlHelper("https://example.com/Account/ConfirmEmail?userId=123&code=test-token");

		_model.UserName = "TestUser";
		_model.Email = "test@example.com";
		_model.Password = "ValidPassword123!";
		_model.ConfirmPassword = "ValidPassword123!";
		_model.TimeZone = "America/New_York";
		_model.Location = null;
		_model.Coppa = true;

		_env.EnvironmentName.Returns("Production");
		_reCaptchaService.VerifyAsync(Arg.Any<string>()).Returns(true);
		_signInManager.UsernameIsAllowed(Arg.Any<string>()).Returns(true);
		_signInManager.EmailExists(Arg.Any<string>()).Returns(false);
		_signInManager.IsPasswordAllowed(_model.UserName, _model.Email, _model.Password).Returns(true);
		_userManager.Create(Arg.Any<User>(), _model.Password).Returns(IdentityResult.Success);
		_userManager.GenerateEmailConfirmationToken(Arg.Any<User>()).Returns("test-token");
	}

	[TestMethod]
	public async Task OnPost_PasswordMismatch_ReturnsPageWithError()
	{
		_model.Password = "ValidPassword123!";
		_model.ConfirmPassword = "DifferentPassword123!";

		var result = await _model.OnPost(_signInManager, _userManager, _emailService, _publisher, _reCaptchaService, _env, _userMaintenanceLogger);

		Assert.IsInstanceOfType(result, typeof(PageResult));
		Assert.IsFalse(_model.ModelState.IsValid);
		Assert.IsTrue(_model.ModelState.ContainsKey(nameof(_model.ConfirmPassword)));
	}

	[TestMethod]
	public async Task OnPost_InvalidCaptchaInProduction_ReturnsPageWithError()
	{
		_reCaptchaService.VerifyAsync(Arg.Any<string>()).Returns(false);

		var result = await _model.OnPost(_signInManager, _userManager, _emailService, _publisher, _reCaptchaService, _env, _userMaintenanceLogger);

		Assert.IsInstanceOfType(result, typeof(PageResult));
		Assert.IsFalse(_model.ModelState.IsValid);
		Assert.IsTrue(_model.ModelState.ErrorCount > 0);
	}

	[TestMethod]
	public async Task OnPost_InvalidLocation_ReturnsPageWithError()
	{
		_model.Location = "InvalidLocation";

		var result = await _model.OnPost(_signInManager, _userManager, _emailService, _publisher, _reCaptchaService, _env, _userMaintenanceLogger);

		Assert.IsInstanceOfType(result, typeof(PageResult));
		Assert.IsFalse(_model.ModelState.IsValid);
		Assert.IsTrue(_model.ModelState.ContainsKey(nameof(_model.Location)));
	}

	[TestMethod]
	public async Task OnPost_UsernameNotAllowed_ReturnsPageWithError()
	{
		_signInManager.UsernameIsAllowed(_model.UserName).Returns(false);

		var result = await _model.OnPost(_signInManager, _userManager, _emailService, _publisher, _reCaptchaService, _env, _userMaintenanceLogger);

		Assert.IsInstanceOfType(result, typeof(PageResult));
		Assert.IsFalse(_model.ModelState.IsValid);
		Assert.IsTrue(_model.ModelState.ContainsKey(nameof(_model.UserName)));
	}

	[TestMethod]
	public async Task OnPost_EmailAlreadyExists_ReturnsPageWithError()
	{
		_signInManager.EmailExists(_model.Email).Returns(true);

		var result = await _model.OnPost(_signInManager, _userManager, _emailService, _publisher, _reCaptchaService, _env, _userMaintenanceLogger);

		Assert.IsInstanceOfType(result, typeof(PageResult));
		Assert.IsFalse(_model.ModelState.IsValid);
		Assert.IsTrue(_model.ModelState.ContainsKey(nameof(_model.Email)));
	}

	[TestMethod]
	public async Task OnPost_PasswordNotAllowed_ReturnsPageWithError()
	{
		_signInManager.IsPasswordAllowed(_model.UserName, _model.Email, _model.Password).Returns(false);

		var result = await _model.OnPost(_signInManager, _userManager, _emailService, _publisher, _reCaptchaService, _env, _userMaintenanceLogger);

		Assert.IsInstanceOfType(result, typeof(PageResult));
		Assert.IsFalse(_model.ModelState.IsValid);
		Assert.IsTrue(_model.ModelState.ContainsKey(nameof(_model.Password)));
	}

	[TestMethod]
	public async Task OnPost_UserCreationFails_ReturnsPageWithErrors()
	{
		_userManager.Create(Arg.Any<User>(), _model.Password).Returns(IdentityResult.Failed(new IdentityError { Description = "Creation failed" }));

		var result = await _model.OnPost(_signInManager, _userManager, _emailService, _publisher, _reCaptchaService, _env, _userMaintenanceLogger);

		Assert.IsInstanceOfType(result, typeof(PageResult));
		Assert.IsFalse(_model.ModelState.IsValid);
	}

	[TestMethod]
	public async Task OnPost_SuccessfulRegistrationWithEmailConfirmationRequired_RedirectsToEmailConfirmationSent()
	{
		_userManager.IsConfirmedEmailRequired().Returns(true);

		var result = await _model.OnPost(_signInManager, _userManager, _emailService, _publisher, _reCaptchaService, _env, _userMaintenanceLogger);

		AssertRedirect(result, "EmailConfirmationSent");
		await _signInManager.Received(1).SignIn(Arg.Any<User>(), false);
		await _publisher.Received(1).Send(Arg.Any<Post>());
		await _userMaintenanceLogger.Received(1).Log(Arg.Any<int>(), Arg.Any<string>());
		await _emailService.Received(1).EmailConfirmation(_model.Email, Arg.Any<string>());
	}

	[TestMethod]
	public async Task OnPost_SuccessfulRegistrationWithoutEmailConfirmationRequired_RedirectsHome()
	{
		_userManager.IsConfirmedEmailRequired().Returns(false);

		var result = await _model.OnPost(_signInManager, _userManager, _emailService, _publisher, _reCaptchaService, _env, _userMaintenanceLogger);

		AssertRedirectHome(result);
		await _signInManager.Received(1).SignIn(Arg.Any<User>(), false);
		await _publisher.Received(1).Send(Arg.Any<Post>());
		await _userMaintenanceLogger.Received(1).Log(Arg.Any<int>(), Arg.Any<string>());
		await _emailService.Received(1).EmailConfirmation(_model.Email, Arg.Any<string>());
	}

	[TestMethod]
	public void HasAllowAnonymousAttribute() => AssertAllowsAnonymousUsers(typeof(RegisterModel));

	[TestMethod]
	public void HasIpBanCheckAttribute() => AssertHasIpBanCheck(typeof(RegisterModel));
}
