using Microsoft.AspNetCore.Identity;
using TASVideos.Core.Services;
using TASVideos.Core.Services.ExternalMediaPublisher;
using TASVideos.Pages.Account;
using TASVideos.Services;

namespace TASVideos.RazorPages.Tests.Pages.Account;

[TestClass]
public class ConfirmEmailTests : BasePageModelTests
{
	private readonly ISignInManager _signInManager;
	private readonly IUserManager _userManager;
	private readonly IExternalMediaPublisher _publisher;
	private readonly IUserMaintenanceLogger _userMaintenanceLogger;
	private readonly ITASVideoAgent _tasVideoAgent;
	private readonly ConfirmEmailModel _model;

	public ConfirmEmailTests()
	{
		_signInManager = Substitute.For<ISignInManager>();
		_userManager = Substitute.For<IUserManager>();
		_publisher = Substitute.For<IExternalMediaPublisher>();
		_userMaintenanceLogger = Substitute.For<IUserMaintenanceLogger>();
		_tasVideoAgent = Substitute.For<ITASVideoAgent>();

		_model = new ConfirmEmailModel(_signInManager, _userManager, _publisher, _userMaintenanceLogger, _tasVideoAgent)
		{
			PageContext = TestPageContext()
		};
	}

	[TestMethod]
	public async Task OnGet_NullUserId_ReturnsHome()
	{
		var result = await _model.OnGet(null, "test-code");
		AssertRedirectHome(result);
	}

	[TestMethod]
	public async Task OnGet_NullCode_ReturnsHome()
	{
		var result = await _model.OnGet("123", null);
		AssertRedirectHome(result);
	}

	[TestMethod]
	public async Task OnGet_UserNotFound_ReturnsHome()
	{
		_userManager.FindById("123").Returns((User?)null);
		var result = await _model.OnGet("123", "test-code");
		AssertRedirectHome(result);
	}

	[TestMethod]
	public async Task OnGet_UserAlreadyConfirmed_ReturnsHome()
	{
		_userManager.FindById("123").Returns(new User
		{
			Id = 123, EmailConfirmed = true
		});

		var result = await _model.OnGet("123", "test-code");

		AssertRedirectHome(result);
		await _userManager.DidNotReceive().ConfirmEmail(Arg.Any<User>(), Arg.Any<string>());
	}

	[TestMethod]
	public async Task OnGet_ConfirmationFailed_RedirectsToError()
	{
		var user = new User
		{
			Id = 123, EmailConfirmed = false
		};
		_userManager.FindById("123").Returns(user);
		_userManager.ConfirmEmail(user, "test-code").Returns(IdentityResult.Failed());

		var result = await _model.OnGet("123", "test-code");

		AssertRedirectError(result);
	}

	[TestMethod]
	public async Task OnGet_SuccessfulConfirmation_CallsFirstTimeConfirmationServicesAndReturnsPage()
	{
		var user = new User
		{
			Id = 123,
			UserName = "TestUser",
			EmailConfirmed = false
		};

		_userManager.FindById("123").Returns(user);
		_userManager.ConfirmEmail(user, "test-code").Returns(IdentityResult.Success);

		var result = await _model.OnGet("123", "test-code");

		Assert.IsInstanceOfType(result, typeof(PageResult));
		await _userManager.Received(1).FindById("123");
		await _userManager.Received(1).ConfirmEmail(user, "test-code");
		await _signInManager.Received(1).SignIn(user, false);
		await _publisher.Received(1).Send(Arg.Any<Post>());
		await _userMaintenanceLogger.Received(1).Log(user.Id, Arg.Any<string>());
		await _tasVideoAgent.Received(1).SendWelcomeMessage(user.Id);
	}

	[TestMethod]
	public void AllowsAnonymousAttribute() => AssertAllowsAnonymousUsers(typeof(ConfirmEmailModel));
}
