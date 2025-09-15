using Microsoft.AspNetCore.Mvc.ViewFeatures;
using TASVideos.Core.Services;
using TASVideos.Core.Services.Email;
using TASVideos.Pages.Account;

namespace TASVideos.RazorPages.Tests.Pages.Account;

[TestClass]
public class EmailConfirmationSentTests : BasePageModelTests
{
	private readonly IUserManager _userManager = Substitute.For<IUserManager>();
	private readonly IEmailService _emailService = Substitute.For<IEmailService>();
	private readonly EmailConfirmationSentModel _model = new()
	{
		PageContext = TestPageContext(),
		TempData = Substitute.For<ITempDataDictionary>(),
		Url = GeMockUrlHelper("https://example.com/Account/ConfirmEmail?userId=123&code=test-token")
	};

	[TestMethod]
	public void OnGet_UserLoggedIn_RedirectsToBaseReturnUrl()
	{
		AddAuthenticatedUser(_model, new User { Id = 1, UserName = "TestUser" }, []);
		var result = _model.OnGet();
		AssertRedirectHome(result);
	}

	[TestMethod]
	public void OnGet_UserNotLoggedIn_ReturnsPage()
	{
		var result = _model.OnGet();
		Assert.IsInstanceOfType(result, typeof(PageResult));
	}

	[TestMethod]
	public async Task OnPost_UserNotFound_DoesNotSendButRedirects()
	{
		_model.Email = "nonexistent@example.com";
		_model.UserName = "NonExistentUser";
		_userManager.GetUserByEmailAndUserName("nonexistent@example.com", "NonExistentUser").Returns((User?)null);

		var result = await _model.OnPost(_userManager, _emailService);

		AssertRedirect(result, "EmailConfirmationSent");
		await _emailService.DidNotReceive().EmailConfirmation(Arg.Any<string>(), Arg.Any<string>());
		Assert.AreEqual("success", _model.MessageType);
	}

	[TestMethod]
	public async Task OnPost_UserFoundButEmailAlreadyConfirmed_DoesNotSendButRedirects()
	{
		var user = new User
		{
			Id = 123,
			UserName = "TestUser",
			Email = "test@example.com",
			EmailConfirmed = true
		};
		_model.Email = "test@example.com";
		_model.UserName = "TestUser";
		_userManager.GetUserByEmailAndUserName("test@example.com", "TestUser").Returns(user);

		var result = await _model.OnPost(_userManager, _emailService);

		AssertRedirect(result, "EmailConfirmationSent");
		await _userManager.DidNotReceive().GenerateEmailConfirmationToken(Arg.Any<User>());
		await _emailService.DidNotReceive().EmailConfirmation(Arg.Any<string>(), Arg.Any<string>());
		Assert.AreEqual("success", _model.MessageType);
	}

	[TestMethod]
	public async Task OnPost_ValidUserWithUnconfirmedEmail_GeneratesTokenSendsEmailAndRedirects()
	{
		var user = new User
		{
			Id = 123,
			UserName = "TestUser",
			Email = "test@example.com",
			EmailConfirmed = false
		};
		_model.Email = "test@example.com";
		_model.UserName = "TestUser";
		_userManager.GetUserByEmailAndUserName("test@example.com", "TestUser").Returns(user);
		_userManager.GenerateEmailConfirmationToken(user).Returns("generated-token");

		var result = await _model.OnPost(_userManager, _emailService);

		AssertRedirect(result, "EmailConfirmationSent");
		await _userManager.Received(1).GetUserByEmailAndUserName("test@example.com", "TestUser");
		await _userManager.Received(1).GenerateEmailConfirmationToken(user);
		await _emailService.Received(1).EmailConfirmation(
			"test@example.com",
			Arg.Any<string>());
		Assert.AreEqual("success", _model.MessageType);
	}

	[TestMethod]
	public void HasAllowAnonymousAttribute() => AssertAllowsAnonymousUsers(typeof(EmailConfirmationSentModel));

	[TestMethod]
	public void HasIpBanCheckAttribute() => AssertHasIpBanCheck(typeof(EmailConfirmationSentModel));
}
