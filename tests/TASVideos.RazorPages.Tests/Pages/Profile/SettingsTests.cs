using System.Security.Claims;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using TASVideos.Core.Services;
using TASVideos.Core.Services.Email;
using TASVideos.Pages.Profile;

namespace TASVideos.RazorPages.Tests.Pages.Profile;

[TestClass]
public class SettingsTests : BasePageModelTests
{
	private readonly IUserManager _userManager = Substitute.For<IUserManager>();
	private readonly IEmailService _emailService = Substitute.For<IEmailService>();
	private readonly SettingsModel _model;

	public SettingsTests()
	{
		_userManager.AvatarSiteIsBanned(Arg.Any<string>()).Returns("");
		_model = new SettingsModel(_userManager, _emailService, _db)
		{
			PageContext = TestPageContext(),
			TempData = Substitute.For<ITempDataDictionary>(),
			Url = GeMockUrlHelper("https://example.com/Account/ConfirmEmail?userId=123&code=test-token")
		};
	}

	[TestMethod]
	public async Task OnGet_PopulatesAllUserProperties()
	{
		var user = new User
		{
			Id = 123,
			UserName = "TestUser",
			Email = "test@example.com",
			EmailConfirmed = true,
			TimeZoneId = "America/New_York",
			PublicRatings = true,
			From = "",
			Signature = "Test Signature",
			Avatar = "https://example.com/avatar.jpg",
			MoodAvatarUrlBase = "https://example.com/mood",
			PreferredPronouns = PreferredPronounTypes.TheyThem,
			EmailOnPrivateMessage = false,
			AutoWatchTopic = UserPreference.Auto,
			DateFormat = UserDateFormat.DDMMY,
			TimeFormat = UserTimeFormat.Clock24Hour,
			DecimalFormat = UserDecimalFormat.Comma
		};
		_userManager.GetRequiredUser(Arg.Any<ClaimsPrincipal>()).Returns(user);
		AddAuthenticatedUser(_model, user, []);

		await _model.OnGet();

		Assert.AreEqual("TestUser", _model.Username);
		Assert.AreEqual("test@example.com", _model.CurrentEmail);
		Assert.IsTrue(_model.IsEmailConfirmed);
		Assert.AreEqual("America/New_York", _model.TimeZone);
		Assert.IsTrue(_model.PublicRatings);
		Assert.AreEqual("", _model.LocationCountry); // Empty From value should result in empty LocationCountry
		Assert.AreEqual("", _model.LocationCustom);
		Assert.AreEqual("Test Signature", _model.Signature);
		Assert.AreEqual("https://example.com/avatar.jpg", _model.AvatarUrl);
		Assert.AreEqual("https://example.com/mood", _model.MoodAvatar);
		Assert.AreEqual(PreferredPronounTypes.TheyThem, _model.PreferredPronouns);
		Assert.IsFalse(_model.EmailOnPrivateMessage);
		Assert.AreEqual(UserPreference.Auto, _model.AutoWatchTopic);
		Assert.AreEqual(UserDateFormat.DDMMY, _model.DateFormat);
		Assert.AreEqual(UserTimeFormat.Clock24Hour, _model.TimeFormat);
		Assert.AreEqual(UserDecimalFormat.Comma, _model.DecimalFormat);
	}

	[TestMethod]
	public async Task OnGet_WithCustomLocation_SetsCustomLocationCorrectly()
	{
		var user = GetAndMockDefaultUser();
		user.From = "Custom Location Name";

		await _model.OnGet();

		Assert.AreEqual(UiDefaults.CustomEntry[0].Value, _model.LocationCountry);
		Assert.AreEqual("Custom Location Name", _model.LocationCustom);
	}

	[TestMethod]
	public async Task OnPost_InvalidLocationCountry_ReturnsPageWithError()
	{
		_model.LocationCountry = "InvalidCountryCode";

		var result = await _model.OnPost();

		Assert.IsInstanceOfType(result, typeof(PageResult));
		Assert.IsFalse(_model.ModelState.IsValid);
		Assert.IsTrue(_model.ModelState.ContainsKey(nameof(_model.LocationCountry)));
	}

	[TestMethod]
	public async Task OnPost_BannedAvatarSite_ReturnsPageWithError()
	{
		_userManager.AvatarSiteIsBanned("https://banned-site.com/avatar.jpg").Returns("banned-site.com");
		_userManager.AvatarSiteIsBanned("").Returns(""); // For MoodAvatar check
		_model.AvatarUrl = "https://banned-site.com/avatar.jpg";

		var result = await _model.OnPost();

		Assert.IsInstanceOfType(result, typeof(PageResult));
		Assert.IsFalse(_model.ModelState.IsValid);
		Assert.IsTrue(_model.ModelState.ContainsKey(nameof(_model.AvatarUrl)));
	}

	[TestMethod]
	public async Task OnPost_BannedMoodAvatarSite_ReturnsPageWithError()
	{
		_userManager.AvatarSiteIsBanned("").Returns(""); // For AvatarUrl check
		_userManager.AvatarSiteIsBanned("https://banned-mood-site.com/mood.jpg").Returns("banned-mood-site.com");
		_model.MoodAvatar = "https://banned-mood-site.com/mood.jpg";

		var result = await _model.OnPost();

		Assert.IsInstanceOfType(result, typeof(PageResult));
		Assert.IsFalse(_model.ModelState.IsValid);
		Assert.IsTrue(_model.ModelState.ContainsKey(nameof(_model.MoodAvatar)));
	}

	[TestMethod]
	public async Task OnPost_SuccessfulUpdate_UpdatesUserAndRedirects()
	{
		var user = GetAndMockDefaultUser();
		AddAuthenticatedUser(_model, user, [PermissionTo.UseMoodAvatars, PermissionTo.EditSignature]);
		_model.TimeZone = "Europe/London";
		_model.PublicRatings = false;
		_model.LocationCountry = ""; // Use empty to avoid location validation issues
		_model.AvatarUrl = "https://example.com/avatar.jpg";
		_model.MoodAvatar = "https://example.com/mood.jpg";
		_model.PreferredPronouns = PreferredPronounTypes.HeHim;
		_model.EmailOnPrivateMessage = true;
		_model.AutoWatchTopic = UserPreference.Always;
		_model.DateFormat = UserDateFormat.DDMMY;
		_model.TimeFormat = UserTimeFormat.Clock24Hour;
		_model.DecimalFormat = UserDecimalFormat.Comma;
		_model.Signature = "Test Signature";

		var result = await _model.OnPost();

		AssertRedirect(result, "Settings");
		Assert.AreEqual("Europe/London", user.TimeZoneId);
		Assert.IsFalse(user.PublicRatings);
		Assert.AreEqual("", user.From);
		Assert.AreEqual("https://example.com/avatar.jpg", user.Avatar);
		Assert.AreEqual("https://example.com/mood.jpg", user.MoodAvatarUrlBase);
		Assert.AreEqual(PreferredPronounTypes.HeHim, user.PreferredPronouns);
		Assert.IsTrue(user.EmailOnPrivateMessage);
		Assert.AreEqual(UserPreference.Always, user.AutoWatchTopic);
		Assert.AreEqual(UserDateFormat.DDMMY, user.DateFormat);
		Assert.AreEqual(UserTimeFormat.Clock24Hour, user.TimeFormat);
		Assert.AreEqual(UserDecimalFormat.Comma, user.DecimalFormat);
		Assert.AreEqual("Test Signature", user.Signature);
	}

	[TestMethod]
	public async Task OnPost_WithoutMoodAvatarPermission_DoesNotSetMoodAvatar()
	{
		var user = GetAndMockDefaultUser();
		_model.MoodAvatar = "https://example.com/mood.jpg";

		var result = await _model.OnPost();

		AssertRedirect(result, "Settings");
		Assert.IsNull(user.MoodAvatarUrlBase);
	}

	[TestMethod]
	public async Task OnPost_WithoutSignaturePermission_DoesNotUpdateSignature()
	{
		var user = GetAndMockDefaultUser();
		user.Signature = "Original Signature";

		AddAuthenticatedUser(_model, user, []);
		_model.Signature = "Updated Signature";
		_model.LocationCountry = "";

		var result = await _model.OnPost();

		AssertRedirect(result, "Settings");
		Assert.AreEqual("Original Signature", user.Signature);
	}

	[TestMethod]
	public async Task OnPost_CustomLocation_SetsLocationFromCustomValue()
	{
		var user = GetAndMockDefaultUser();
		AddAuthenticatedUser(_model, user, []);
		_model.LocationCountry = UiDefaults.CustomEntry[0].Value;
		_model.LocationCustom = "My Custom Location";

		var result = await _model.OnPost();

		AssertRedirect(result, "Settings");
		Assert.AreEqual("My Custom Location", user.From);
	}

	[TestMethod]
	public async Task OnPostSendVerificationEmail_InvalidModelState_ReturnsPage()
	{
		_model.ModelState.AddModelError("Test", "Test error");
		var result = await _model.OnPostSendVerificationEmail();
		Assert.IsInstanceOfType(result, typeof(PageResult));
	}

	[TestMethod]
	public async Task OnPostSendVerificationEmail_ValidRequest_GeneratesTokenSendsEmailAndRedirects()
	{
		var user = GetAndMockDefaultUser();
		_userManager.GenerateEmailConfirmationToken(user).Returns("generated-token");

		var result = await _model.OnPostSendVerificationEmail();

		AssertRedirect(result, "Settings");
		await _userManager.Received(1).GenerateEmailConfirmationToken(user);
		await _emailService.Received(1).EmailConfirmation("test@example.com", Arg.Any<string>());
		Assert.AreEqual("success", _model.MessageType);
	}

	[TestMethod]
	public void HasAuthorizeAttribute() => AssertRequiresAuthorization(typeof(SettingsModel));

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
