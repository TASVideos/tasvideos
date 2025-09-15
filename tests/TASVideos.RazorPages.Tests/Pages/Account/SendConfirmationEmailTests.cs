using System.Security.Claims;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using TASVideos.Core.Services;
using TASVideos.Core.Services.Email;
using TASVideos.Pages.Account;

namespace TASVideos.RazorPages.Tests.Pages.Account;

[TestClass]
public class SendConfirmationEmailTests : BasePageModelTests
{
	private readonly IUserManager _userManager = Substitute.For<IUserManager>();
	private readonly IEmailService _emailService = Substitute.For<IEmailService>();
	private readonly SendConfirmationEmail _model;

	public SendConfirmationEmailTests()
	{
		_model = new SendConfirmationEmail(_userManager, _emailService)
		{
			PageContext = TestPageContext(),
			TempData = Substitute.For<ITempDataDictionary>(),
			Url = GeMockUrlHelper("https://example.com/Account/ResetPassword?userId=123&code=test-code")
		};
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
		_userManager.GetRequiredUser(Arg.Any<ClaimsPrincipal>()).Returns(user);
		_userManager.GenerateEmailConfirmationToken(user).Returns("generated-token");

		var result = await _model.OnPost();

		AssertRedirect(result, "EmailConfirmationSent");
		await _userManager.Received(1).GenerateEmailConfirmationToken(user);
		await _emailService.Received(1).EmailConfirmation(
			"test@example.com",
			Arg.Any<string>());
	}

	[TestMethod]
	public void HasAuthorizeAttribute() => AssertRequiresAuthorization(typeof(SendConfirmationEmail));

	[TestMethod]
	public void HasIpBanCheckAttribute() => AssertHasIpBanCheck(typeof(SendConfirmationEmail));
}
