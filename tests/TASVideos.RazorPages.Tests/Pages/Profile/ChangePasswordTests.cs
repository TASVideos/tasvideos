using System.Security.Claims;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using TASVideos.Core.Services;
using TASVideos.Core.Services.Email;
using TASVideos.Pages.Profile;

namespace TASVideos.RazorPages.Tests.Pages.Profile;

[TestClass]
public class ChangePasswordTests : BasePageModelTests
{
	private readonly IEmailService _emailService = Substitute.For<IEmailService>();
	private readonly ISignInManager _signInManager = Substitute.For<ISignInManager>();
	private readonly IUserManager _userManager = Substitute.For<IUserManager>();
	private readonly ChangePasswordModel _model;

	public ChangePasswordTests()
	{
		_model = new ChangePasswordModel(_emailService, _signInManager, _userManager)
		{
			PageContext = TestPageContext(),
			TempData = Substitute.For<ITempDataDictionary>(),
			Url = GeMockUrlHelper("https://example.com/Account/ResetPassword?userId=123&code=test-code")
		};
	}

	[TestMethod]
	public async Task OnGet_UserHasPassword_ReturnsPage()
	{
		_signInManager.HasPassword(Arg.Any<ClaimsPrincipal>()).Returns(true);
		var result = await _model.OnGet();
		Assert.IsInstanceOfType(result, typeof(PageResult));
	}

	[TestMethod]
	public async Task OnGet_UserHasNoPassword_RedirectsToSetPassword()
	{
		_signInManager.HasPassword(Arg.Any<ClaimsPrincipal>()).Returns(false);
		var result = await _model.OnGet();
		AssertRedirect(result, "SetPassword");
	}

	[TestMethod]
	public async Task OnPost_InvalidModelState_ReturnsPage()
	{
		_model.ModelState.AddModelError("NewPassword", "Password too short");

		var result = await _model.OnPost();

		Assert.IsInstanceOfType(result, typeof(PageResult));
		Assert.IsFalse(_model.ModelState.IsValid);
	}

	[TestMethod]
	public async Task OnPost_PasswordNotAllowed_ReturnsPageWithError()
	{
		var user = GetAndMockDefaultUser();
		SetupValidPasswordModel();
		_signInManager.IsPasswordAllowed(user.UserName, user.Email, _model.NewPassword).Returns(false);

		var result = await _model.OnPost();

		Assert.IsInstanceOfType(result, typeof(PageResult));
		Assert.IsFalse(_model.ModelState.IsValid);
		Assert.IsTrue(_model.ModelState.ContainsKey(nameof(_model.NewPassword)));
	}

	[TestMethod]
	public async Task OnPost_PasswordChangeFails_ReturnsPageWithErrors()
	{
		var user = GetAndMockDefaultUser();
		SetupValidPasswordModel();
		_signInManager.IsPasswordAllowed(user.UserName, user.Email, _model.NewPassword).Returns(true);
		_userManager.ChangePassword(user, _model.CurrentPassword, _model.NewPassword)
			.Returns(IdentityResult.Failed(new IdentityError { Description = "Current password is incorrect" }));

		var result = await _model.OnPost();

		Assert.IsInstanceOfType(result, typeof(PageResult));
		Assert.IsFalse(_model.ModelState.IsValid);
		await _signInManager.DidNotReceive().SignIn(Arg.Any<User>(), Arg.Any<bool>());
		await _emailService.DidNotReceive().PasswordResetConfirmation(Arg.Any<string>(), Arg.Any<string>());
	}

	[TestMethod]
	public async Task OnPost_PasswordChangeSucceeds_SignsInAndRedirectsWithSuccessMessage()
	{
		var user = GetAndMockDefaultUser();
		SetupValidPasswordModel();
		_signInManager.IsPasswordAllowed(user.UserName, user.Email, _model.NewPassword).Returns(true);
		_userManager.ChangePassword(user, _model.CurrentPassword, _model.NewPassword).Returns(IdentityResult.Success);
		_userManager.GeneratePasswordResetToken(user).Returns("reset-token");

		var result = await _model.OnPost();

		AssertRedirect(result, "Index");
		await _signInManager.Received(1).SignIn(user, false);
		await _userManager.Received(1).GeneratePasswordResetToken(user);
		await _emailService.Received(1).PasswordResetConfirmation(user.Email, Arg.Any<string>());
		Assert.AreEqual("success", _model.MessageType);
	}

	[TestMethod]
	public void HasAuthorizeAttribute() => AssertRequiresAuthorization(typeof(ChangePasswordModel));

	private void SetupValidPasswordModel()
	{
		_model.CurrentPassword = "CurrentPassword123!";
		_model.NewPassword = "NewValidPassword123!";
		_model.ConfirmNewPassword = "NewValidPassword123!";
	}

	private User GetAndMockDefaultUser()
	{
		var user = new User
		{
			Id = 123,
			UserName = "TestUser",
			Email = "test@example.com"
		};

		_userManager.GetRequiredUser(Arg.Any<ClaimsPrincipal>()).Returns(user);
		AddAuthenticatedUser(_model, user, []);

		return user;
	}
}
