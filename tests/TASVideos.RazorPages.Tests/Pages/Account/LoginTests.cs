using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.Extensions.Hosting;
using TASVideos.Core.Services;
using TASVideos.Pages.Account;
using static Microsoft.AspNetCore.Identity.SignInResult;

namespace TASVideos.RazorPages.Tests.Pages.Account;

[TestClass]
public class LoginTests : BasePageModelTests
{
	private readonly ISignInManager _signInManager = Substitute.For<ISignInManager>();
	private readonly IUserManager _userManager = Substitute.For<IUserManager>();
	private readonly IHostEnvironment _env = Substitute.For<IHostEnvironment>();

	private readonly LoginModel _model = new()
	{
		PageContext = TestPageContext(),
		TempData = Substitute.For<ITempDataDictionary>()
	};

	[TestMethod]
	public async Task OnGet_UserNotAuthenticated_SignsOutAndReturnsPage()
	{
		var authService = Substitute.For<IAuthenticationService>();
		var serviceProvider = Substitute.For<IServiceProvider>();
		serviceProvider.GetService(typeof(IAuthenticationService)).Returns(authService);
		_model.HttpContext.RequestServices = serviceProvider;

		var result = await _model.OnGet();

		Assert.IsInstanceOfType(result, typeof(PageResult));
		await authService.Received(1).SignOutAsync(_model.HttpContext, Arg.Any<string>(), Arg.Any<AuthenticationProperties>());
	}

	[TestMethod]
	public async Task OnGet_UserAuthenticated_Redirects()
	{
		var user = _db.AddUser("TestUser").Entity;
		await _db.SaveChangesAsync();
		AddAuthenticatedUser(_model, user, [PermissionTo.CreateAdditionalMovieFiles]);

		var result = await _model.OnGet();

		AssertRedirectHome(result);
	}

	[TestMethod]
	public async Task OnPost_InvalidModelState_ReturnsPage()
	{
		_model.ModelState.AddModelError("UserName", "Username is required");
		var result = await _model.OnPost(_signInManager, _userManager, _env);
		Assert.IsInstanceOfType(result, typeof(PageResult));
	}

	[TestMethod]
	public async Task OnPost_SuccessfulLogin_ReturnsBaseReturnUrlRedirect()
	{
		var user = new User { Id = 123, UserName = "TestUser" };
		_model.UserName = "TestUser";
		_model.Password = "TestPassword";
		_model.RememberMe = true;
		_signInManager.SignIn("TestUser", "TestPassword", true).Returns((Success, user, false));

		var result = await _model.OnPost(_signInManager, _userManager, _env);

		AssertRedirectHome(result);
	}

	[TestMethod]
	public async Task OnPost_FailedDueToBan_RedirectsToBannedPage()
	{
		var user = new User { Id = 123, UserName = "BannedUser" };
		_model.UserName = "BannedUser";
		_model.Password = "TestPassword";
		_signInManager.SignIn("BannedUser", "TestPassword").Returns((Failed, user, true));
		var result = await _model.OnPost(_signInManager, _userManager, _env);

		AssertRedirect(result, "/Account/Banned");
		Assert.AreEqual(123, _model.TempData[nameof(BannedModel.BannedUserId)]);
	}

	[TestMethod]
	public async Task OnPost_EmailNotConfirmedInProduction_RedirectsToEmailConfirmationSent()
	{
		var user = new User { Id = 123, UserName = "UnconfirmedUser" };
		_env.EnvironmentName.Returns("Production");
		_userManager.IsEmailConfirmed(user).Returns(false);
		_model.UserName = "UnconfirmedUser";
		_model.Password = "TestPassword";
		_signInManager.SignIn("UnconfirmedUser", "TestPassword").Returns((Failed, (User?)user, false));

		var result = await _model.OnPost(_signInManager, _userManager, _env);

		AssertRedirect(result, "/Account/EmailConfirmationSent");
	}

	[TestMethod]
	public async Task OnPost_EmailNotConfirmedInDevelopment_DoesNotRedirect()
	{
		var user = new User { Id = 123, UserName = "UnconfirmedUser" };
		_env.EnvironmentName.Returns("Development");
		_userManager.IsEmailConfirmed(user).Returns(false);
		_model.UserName = "UnconfirmedUser";
		_model.Password = "TestPassword";
		_signInManager.SignIn("UnconfirmedUser", "TestPassword").Returns((Failed, (User?)user, false));

		var result = await _model.OnPost(_signInManager, _userManager, _env);

		Assert.IsInstanceOfType(result, typeof(PageResult));
		Assert.IsTrue(_model.ModelState.ContainsKey(""));
	}

	[TestMethod]
	public async Task OnPost_AccountLockedOut_RedirectsToLockout()
	{
		var user = new User { Id = 123, UserName = "LockedUser" };
		_env.EnvironmentName.Returns("Production");
		_userManager.IsEmailConfirmed(user).Returns(true);
		_model.UserName = "LockedUser";
		_model.Password = "TestPassword";
		_signInManager.SignIn("LockedUser", "TestPassword").Returns((LockedOut, (User?)user, false));

		var result = await _model.OnPost(_signInManager, _userManager, _env);

		AssertRedirect(result, "/Account/Lockout");
	}

	[TestMethod]
	public async Task OnPost_GeneralFailure_AddsModelErrorAndReturnsPage()
	{
		var user = new User { Id = 123, UserName = "TestUser" };
		_env.EnvironmentName.Returns("Production");
		_userManager.IsEmailConfirmed(user).Returns(true);
		_model.UserName = "TestUser";
		_model.Password = "WrongPassword";
		_signInManager.SignIn("TestUser", "WrongPassword").Returns((Failed, (User?)user, false));

		var result = await _model.OnPost(_signInManager, _userManager, _env);

		Assert.IsInstanceOfType(result, typeof(PageResult));
		Assert.IsTrue(_model.ModelState.ContainsKey(""));
		Assert.IsNotNull(_model.ModelState[""]);
		Assert.AreEqual("Invalid login attempt.", _model.ModelState[""]!.Errors[0].ErrorMessage);
	}

	[TestMethod]
	public async Task OnPost_NullUser_AddsModelErrorAndReturnsPage()
	{
		_model.UserName = "NonexistentUser";
		_model.Password = "TestPassword";
		_signInManager.SignIn("NonexistentUser", "TestPassword").Returns((Failed, (User?)null, false));

		var result = await _model.OnPost(_signInManager, _userManager, _env);

		Assert.IsInstanceOfType(result, typeof(PageResult));
		Assert.IsTrue(_model.ModelState.ContainsKey(""));
		Assert.IsNotNull(_model.ModelState[""]);
		Assert.AreEqual("Invalid login attempt.", _model.ModelState[""]!.Errors[0].ErrorMessage);
	}

	[TestMethod]
	public async Task OnPost_EmailConfirmedUser_DoesNotRedirectToEmailConfirmation()
	{
		var user = new User { Id = 123, UserName = "ConfirmedUser" };
		_env.EnvironmentName.Returns("Production");
		_userManager.IsEmailConfirmed(user).Returns(true);
		_model.UserName = "ConfirmedUser";
		_model.Password = "WrongPassword";
		_signInManager.SignIn("ConfirmedUser", "WrongPassword").Returns((Failed, (User?)user, false));

		var result = await _model.OnPost(_signInManager, _userManager, _env);

		Assert.IsInstanceOfType(result, typeof(PageResult));
		Assert.IsTrue(_model.ModelState.ContainsKey(""));
	}

	[TestMethod]
	public void AllowsAnonymousAttribute() => AssertAllowsAnonymousUsers(typeof(LoginModel));

	[TestMethod]
	public void HasIpBanCheckAttribute() => AssertHasIpBanCheck(typeof(LoginModel));
}
