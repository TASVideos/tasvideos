using System.Security.Claims;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using TASVideos.Core.Services;
using TASVideos.Core.Services.Email;
using TASVideos.Pages.Profile;

namespace TASVideos.RazorPages.Tests.Pages.Profile;

[TestClass]
public class ChangeEmailTests : BasePageModelTests
{
	private readonly IUserManager _userManager = Substitute.For<IUserManager>();
	private readonly ICacheService _cache = Substitute.For<ICacheService>();
	private readonly IEmailService _emailService = Substitute.For<IEmailService>();
	private readonly ChangeEmailModel _model;

	public ChangeEmailTests()
	{
		_model = new ChangeEmailModel(_userManager, _cache, _emailService)
		{
			PageContext = TestPageContext(),
			TempData = Substitute.For<ITempDataDictionary>(),
			Url = GeMockUrlHelper("https://example.com/Account/ConfirmEmailChange?token=test-token")
		};
	}

	[TestMethod]
	public async Task OnGet_PopulatesUserEmailInfo()
	{
		var user = new User
		{
			Id = 123,
			UserName = "TestUser",
			Email = "test@example.com",
			EmailConfirmed = true
		};
		_userManager.GetRequiredUser(Arg.Any<ClaimsPrincipal>()).Returns(user);

		await _model.OnGet();

		Assert.AreEqual(user.Email, _model.CurrentEmail);
		Assert.IsTrue(_model.IsEmailConfirmed);
	}

	[TestMethod]
	public async Task OnPost_InvalidModelState_ReturnsPage()
	{
		_model.ModelState.AddModelError(nameof(_model.NewEmail), "Invalid email format");

		var result = await _model.OnPost();

		Assert.IsInstanceOfType(result, typeof(PageResult));
		Assert.IsFalse(_model.ModelState.IsValid);
	}

	[TestMethod]
	[DataRow(null)]
	[DataRow("")]
	[DataRow("   ")]
	public async Task OnPost_TokenGenerationFails_ReturnsBadRequest(string token)
	{
		_userManager.GenerateChangeEmailToken(Arg.Any<ClaimsPrincipal>(), Arg.Any<string>()).Returns(token);
		var result = await _model.OnPost();
		AssertBadRequest(result);
	}

	[TestMethod]
	public async Task OnPost_ValidRequest_CachesEmailSendsConfirmationAndRedirects()
	{
		_model.NewEmail = "new@example.com";
		_userManager.GenerateChangeEmailToken(Arg.Any<ClaimsPrincipal>(), "new@example.com").Returns("generated-token");

		var result = await _model.OnPost();

		AssertRedirect(result, "EmailConfirmationSent");
		_cache.Received(1).Set("generated-token", "new@example.com");
		await _emailService.Received(1).EmailConfirmation("new@example.com", Arg.Any<string>());
	}

	[TestMethod]
	public void HasAuthorizeAttribute() => AssertRequiresAuthorization(typeof(ChangeEmailModel));
}
