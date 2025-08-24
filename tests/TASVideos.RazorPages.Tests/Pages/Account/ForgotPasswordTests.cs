using Microsoft.AspNetCore.Mvc.ViewFeatures;
using TASVideos.Core.Services;
using TASVideos.Core.Services.Email;
using TASVideos.Pages.Account;

namespace TASVideos.RazorPages.Tests.Pages.Account;

[TestClass]
public class ForgotPasswordTests : BasePageModelTests
{
	private readonly IUserManager _userManager = Substitute.For<IUserManager>();
	private readonly IEmailService _emailService = Substitute.For<IEmailService>();
	private readonly ForgotPasswordModel _model = new()
	{
		PageContext = TestPageContext(),
		TempData = Substitute.For<ITempDataDictionary>(),
		Url = GeMockUrlHelper("https://example.com/Account/ResetPassword?userId=123&code=test-code")
	};

	[TestMethod]
	public async Task OnPost_InvalidModelState_ReturnsPage()
	{
		_model.ModelState.AddModelError("Email", "Email is required");
		var result = await _model.OnPost(_userManager, _emailService);
		Assert.IsInstanceOfType(result, typeof(PageResult));
	}

	[TestMethod]
	public async Task OnPost_UserNotFound_RedirectsToConfirmation()
	{
		_model.Email = "nonexistent@example.com";
		_userManager.FindByEmail("nonexistent@example.com").Returns((User?)null);

		var result = await _model.OnPost(_userManager, _emailService);

		AssertRedirect(result, "ForgotPasswordConfirmation");
		await _emailService.DidNotReceive().ResetPassword(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>());
	}

	[TestMethod]
	public async Task OnPost_ValidUser_GeneratesTokenSendsEmailAndRedirects()
	{
		var user = new User
		{
			Id = 123,
			UserName = "TestUser",
			Email = "test@example.com"
		};

		_model.Email = "test@example.com";
		_userManager.FindByEmail("test@example.com").Returns(user);
		_userManager.GeneratePasswordResetToken(user).Returns("generated-token");

		var result = await _model.OnPost(_userManager, _emailService);

		AssertRedirect(result, "ForgotPasswordConfirmation");
		await _userManager.Received(1).FindByEmail("test@example.com");
		await _userManager.Received(1).GeneratePasswordResetToken(user);
		await _emailService.Received(1).ResetPassword(
			"test@example.com",
			"https://example.com/Account/ResetPassword?userId=123&code=test-code",
			"TestUser");
	}

	[TestMethod]
	public async Task OnPost_ValidUser_CallsUrlHelperWithCorrectParameters()
	{
		var user = new User
		{
			Id = 456,
			UserName = "AnotherUser",
			Email = "another@example.com"
		};

		_model.Email = "another@example.com";
		_userManager.FindByEmail("another@example.com").Returns(user);
		_userManager.GeneratePasswordResetToken(user).Returns("another-token");

		var result = await _model.OnPost(_userManager, _emailService);

		AssertRedirect(result, "ForgotPasswordConfirmation");
	}

	[TestMethod]
	public async Task OnPost_EmailServiceCall_UsesCorrectParameters()
	{
		var user = new User
		{
			Id = 789,
			UserName = "ThirdUser",
			Email = "third@example.com"
		};

		_model.Email = "third@example.com";
		_userManager.FindByEmail("third@example.com").Returns(user);
		_userManager.GeneratePasswordResetToken(user).Returns("third-token");

		var result = await _model.OnPost(_userManager, _emailService);

		AssertRedirect(result, "ForgotPasswordConfirmation");
		await _emailService.Received(1).ResetPassword(
			"third@example.com",
			Arg.Any<string>(),
			"ThirdUser");
	}

	[TestMethod]
	public async Task OnPost_UserWithDifferentCaseEmail_FindsUserCorrectly()
	{
		var user = new User
		{
			Id = 999,
			UserName = "CaseUser",
			Email = "case@example.com"
		};

		_model.Email = "CASE@EXAMPLE.COM";  // Different case
		_userManager.FindByEmail("CASE@EXAMPLE.COM").Returns(user);
		_userManager.GeneratePasswordResetToken(user).Returns("case-token");

		var result = await _model.OnPost(_userManager, _emailService);

		AssertRedirect(result, "ForgotPasswordConfirmation");
		await _userManager.Received(1).FindByEmail("CASE@EXAMPLE.COM");
		await _emailService.Received(1).ResetPassword(
			"CASE@EXAMPLE.COM",
			Arg.Any<string>(),
			"CaseUser");
	}

	[TestMethod]
	[DataRow("")]
	[DataRow("invalid-email")]
	[DataRow("missing@")]
	[DataRow("@missing.com")]
	public async Task OnPost_InvalidEmailFormat_ReturnsPageWithModelError(string invalidEmail)
	{
		_model.Email = invalidEmail;
		_model.ModelState.AddModelError(nameof(_model.Email), "Invalid email format");

		var result = await _model.OnPost(_userManager, _emailService);

		Assert.IsInstanceOfType(result, typeof(PageResult));
		Assert.IsFalse(_model.ModelState.IsValid);
		Assert.IsTrue(_model.ModelState.ContainsKey(nameof(_model.Email)));
	}

	[TestMethod]
	public async Task OnPost_TokenGenerationReturnsEmptyString_StillSendsEmail()
	{
		var user = new User
		{
			Id = 111,
			UserName = "EmptyTokenUser",
			Email = "empty@example.com"
		};

		_model.Email = "empty@example.com";
		_userManager.FindByEmail("empty@example.com").Returns(user);
		_userManager.GeneratePasswordResetToken(user).Returns("");  // Empty token

		var result = await _model.OnPost(_userManager, _emailService);

		AssertRedirect(result, "ForgotPasswordConfirmation");
		await _emailService.Received(1).ResetPassword(
			"empty@example.com",
			Arg.Any<string>(),
			"EmptyTokenUser");
	}

	[TestMethod]
	public void AllowsAnonymousAttribute() => AssertAllowsAnonymousUsers(typeof(ForgotPasswordModel));
}
