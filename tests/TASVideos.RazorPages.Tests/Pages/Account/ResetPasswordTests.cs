using Microsoft.AspNetCore.Identity;
using TASVideos.Core.Services;
using TASVideos.Pages.Account;

namespace TASVideos.RazorPages.Tests.Pages.Account;

[TestClass]
public class ResetPasswordTests : BasePageModelTests
{
	private readonly IUserManager _userManager;
	private readonly ResetPasswordModel _model;

	public ResetPasswordTests()
	{
		_userManager = Substitute.For<IUserManager>();
		_model = new ResetPasswordModel(_userManager)
		{
			PageContext = TestPageContext()
		};
	}

	[TestMethod]
	[DataRow(null)]
	[DataRow("")]
	[DataRow("   ")]
	public async Task OnGet_NullOrWhitespaceCode_ReturnsHome(string? code)
	{
		_model.Code = code;
		var result = await _model.OnGet();
		AssertRedirectHome(result);
	}

	[TestMethod]
	public async Task OnGet_UserNotFound_ReturnsHome()
	{
		_model.Code = "valid-code";
		_model.UserId = "123";
		_userManager.FindById("123").Returns((User?)null);

		var result = await _model.OnGet();

		AssertRedirectHome(result);
	}

	[TestMethod]
	public async Task OnGet_InvalidToken_ReturnsHome()
	{
		var user = new User { Id = 123 };
		_model.Code = "invalid-code";
		_model.UserId = "123";
		_userManager.FindById("123").Returns(user);
		_userManager.VerifyUserToken(user, "invalid-code").Returns(false);

		var result = await _model.OnGet();

		AssertRedirectHome(result);
		await _userManager.Received(1).VerifyUserToken(user, "invalid-code");
	}

	[TestMethod]
	public async Task OnGet_ValidTokenAndUser_SetsPropertiesAndReturnsPage()
	{
		var user = new User
		{
			Id = 123,
			UserName = "TestUser",
			Email = "test@example.com"
		};
		_model.Code = "valid-code";
		_model.UserId = "123";
		_userManager.FindById("123").Returns(user);
		_userManager.VerifyUserToken(user, "valid-code").Returns(true);

		var result = await _model.OnGet();

		Assert.IsInstanceOfType(result, typeof(PageResult));
		Assert.AreEqual("test@example.com", _model.Email);
		Assert.AreEqual("TestUser", _model.UserName);
	}

	[TestMethod]
	public async Task OnPost_InvalidModelState_ReturnsPage()
	{
		_model.ModelState.AddModelError("NewPassword", "Password is required");
		var result = await _model.OnPost();
		Assert.IsInstanceOfType(result, typeof(PageResult));
	}

	[TestMethod]
	[DataRow(null)]
	[DataRow("")]
	[DataRow("   ")]
	public async Task OnPost_NullOrWhitespaceCode_ReturnsHome(string? code)
	{
		_model.Code = code;
		var result = await _model.OnPost();
		AssertRedirectHome(result);
	}

	[TestMethod]
	public async Task OnPost_UserNotFound_ReturnsHome()
	{
		_model.Code = "valid-code";
		_model.UserId = "123";
		_model.NewPassword = "NewPassword123!";
		_userManager.FindById("123").Returns((User?)null);

		var result = await _model.OnPost();

		AssertRedirectHome(result);
	}

	[TestMethod]
	public async Task OnPost_SuccessfulReset_MarksEmailConfirmedAndRedirects()
	{
		var user = new User
		{
			Id = 123,
			UserName = "TestUser",
			Email = "test@example.com"
		};
		_model.Code = "valid-code";
		_model.UserId = "123";
		_model.NewPassword = "NewPassword123!";
		_userManager.FindById("123").Returns(user);
		_userManager.ResetPasswordAsync(user, "valid-code", "NewPassword123!").Returns(IdentityResult.Success);

		var result = await _model.OnPost();

		AssertRedirect(result, "ResetPasswordConfirmation");
		await _userManager.Received(1).FindById("123");
		await _userManager.Received(1).ResetPasswordAsync(user, "valid-code", "NewPassword123!");
		await _userManager.Received(1).MarkEmailConfirmed(user);
	}

	[TestMethod]
	public async Task OnPost_FailedReset_AddsErrorsAndReturnsPage()
	{
		var user = new User
		{
			Id = 123,
			UserName = "TestUser",
			Email = "test@example.com"
		};
		_model.Code = "valid-code";
		_model.UserId = "123";
		_model.NewPassword = "NewPassword123!";
		_userManager.FindById("123").Returns(user);
		_userManager.ResetPasswordAsync(user, "valid-code", "NewPassword123!")
			.Returns(IdentityResult.Failed(new IdentityError { Description = "Password reset failed" }));

		var result = await _model.OnPost();

		Assert.IsInstanceOfType(result, typeof(PageResult));
		await _userManager.Received(1).FindById("123");
		await _userManager.Received(1).ResetPasswordAsync(user, "valid-code", "NewPassword123!");
		await _userManager.DidNotReceive().MarkEmailConfirmed(Arg.Any<User>());
		Assert.IsTrue(_model.ModelState.ContainsKey(""));
	}

	[TestMethod]
	public void HasIpBanCheckAttribute() => AssertHasIpBanCheck(typeof(ResetPasswordModel));

	[TestMethod]
	public void AllowsAnonymousAttribute() => AssertAllowsAnonymousUsers(typeof(ResetPasswordModel));
}
