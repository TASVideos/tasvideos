using System.Security.Claims;
using Microsoft.AspNetCore.Identity;
using TASVideos.Core.Services;
using TASVideos.Core.Services.ExternalMediaPublisher;
using TASVideos.Pages.Account;
using TASVideos.Services;

namespace TASVideos.RazorPages.Tests.Pages.Account;

[TestClass]
public class ConfirmEmailChangeTests : BasePageModelTests
{
	private readonly IUserManager _userManager = Substitute.For<IUserManager>();
	private readonly IUserMaintenanceLogger _userMaintenanceLogger = Substitute.For<IUserMaintenanceLogger>();
	private readonly ICacheService _cache = Substitute.For<ICacheService>();
	private readonly ISignInManager _signInManager = Substitute.For<ISignInManager>();
	private readonly IExternalMediaPublisher _publisher = Substitute.For<IExternalMediaPublisher>();
	private readonly ITASVideoAgent _tasVideoAgent = Substitute.For<ITASVideoAgent>();

	private readonly ConfirmEmailChangeModel _model;

	public ConfirmEmailChangeTests()
	{
		_model = new ConfirmEmailChangeModel(_userManager, _userMaintenanceLogger, _cache, _signInManager, _publisher, _tasVideoAgent);
	}

	[TestMethod]
	[DataRow(null)]
	[DataRow("")]
	[DataRow("   ")]
	public async Task OnGet_NullOrWhitespaceCode_ReturnsAccessDenied(string code)
	{
		var result = await _model.OnGet(code);
		AssertAccessDenied(result);
	}

	[TestMethod]
	public async Task OnGet_CodeNotInCache_ReturnsBadRequest()
	{
		_cache.TryGetValue("invalid-code", out Arg.Any<string>()).Returns(false);
		var result = await _model.OnGet("invalid-code");
		AssertBadRequest(result);
	}

	[TestMethod]
	public async Task OnGet_ChangeEmailFails_ReturnsError()
	{
		var user = _db.AddUser("TestUser").Entity;
		await _db.SaveChangesAsync();

		const string code = "valid-code";
		const string newEmail = "newemail@example.com";

		_userManager.GetRequiredUser(Arg.Any<ClaimsPrincipal>()).Returns(user);
		_cache.TryGetValue(code, out Arg.Any<string>()).Returns(x =>
		{
			x[1] = newEmail;
			return true;
		});
		_userManager.ChangeEmail(user, newEmail, code).Returns(IdentityResult.Failed());

		var result = await _model.OnGet(code);

		AssertRedirectError(result);
	}

	[TestMethod]
	public async Task OnGet_SuccessfulEmailChange_PreviouslyConfirmed_LogsMaintenanceAndRedirects()
	{
		var user = _db.AddUser("TestUser").Entity;
		user.EmailConfirmed = true;
		await _db.SaveChangesAsync();
		AddAuthenticatedUser(_model, user, []);

		const string code = "valid-code";
		const string newEmail = "newemail@example.com";

		_userManager.GetRequiredUser(Arg.Any<ClaimsPrincipal>()).Returns(user);
		_cache.TryGetValue(code, out Arg.Any<string>()).Returns(x =>
		{
			x[1] = newEmail;
			return true;
		});
		_userManager.ChangeEmail(user, newEmail, code).Returns(IdentityResult.Success);

		var result = await _model.OnGet(code);

		AssertRedirect(result, "/Profile/Settings");

		_cache.Received(1).Remove(code);
		await _userMaintenanceLogger.Received(1).Log(user.Id, Arg.Is<string>(s => s.Contains("User changed email from")));
		await _userManager.DidNotReceive().AddStandardRoles(Arg.Any<int>());
		await _userManager.DidNotReceive().AddUserPermissionsToClaims(Arg.Any<User>());
		await _signInManager.DidNotReceive().SignIn(Arg.Any<User>(), Arg.Any<bool>());
		await _publisher.DidNotReceive().Send(Arg.Any<Post>());
		await _tasVideoAgent.DidNotReceive().SendWelcomeMessage(Arg.Any<int>());
	}

	[TestMethod]
	public async Task OnGet_SuccessfulEmailChange_NotPreviouslyConfirmed_CallsFirstTimeConfirmation()
	{
		var user = _db.AddUser("TestUser").Entity;
		user.EmailConfirmed = false;
		await _db.SaveChangesAsync();
		AddAuthenticatedUser(_model, user, []);

		const string code = "valid-code";
		const string newEmail = "newemail@example.com";

		_userManager.GetRequiredUser(Arg.Any<ClaimsPrincipal>()).Returns(user);
		_cache.TryGetValue(code, out Arg.Any<string>()).Returns(x =>
		{
			x[1] = newEmail;
			return true;
		});
		_userManager.ChangeEmail(user, newEmail, code).Returns(IdentityResult.Success);

		var result = await _model.OnGet(code);

		AssertRedirect(result, "/Profile/Settings");

		_cache.Received(1).Remove(code);
		await _userManager.Received(1).AddStandardRoles(user.Id);
		await _userManager.Received(1).AddUserPermissionsToClaims(user);
		await _signInManager.Received(1).SignIn(user, false);
		await _publisher.Received(1).Send(Arg.Any<Post>());
		await _userMaintenanceLogger.Received(1).Log(user.Id, Arg.Is<string>(s => s.Contains("User activated from")));
		await _tasVideoAgent.Received(1).SendWelcomeMessage(user.Id);
		Assert.AreEqual("success", _model.MessageType);
	}

	[TestMethod]
	public void HasAuthorizeAttribute() => AssertRequiresAuthorization(typeof(ConfirmEmailChangeModel));
}
