using TASVideos.Pages.Account;

namespace TASVideos.RazorPages.Tests.Pages.Account;

[TestClass]
public class BannedTests : BasePageModelTests
{
	private readonly BannedModel _model;

	public BannedTests()
	{
		_model = new BannedModel(_db);
	}

	[TestMethod]
	[DataRow(null)]
	[DataRow(999)]
	public async Task OnGet_UserNullOrNotFound_ReturnsHome(int? userId)
	{
		_model.BannedUserId = 999;

		var result = await _model.OnGet();

		AssertRedirectHome(result);
	}

	[TestMethod]
	public async Task OnGet_ValidBannedUser_SetsPropertiesAndReturnsPage()
	{
		var bannedUntil = DateTime.UtcNow.AddDays(30);
		var user = _db.AddUser("BannedUser").Entity;
		user.BannedUntil = bannedUntil;
		await _db.SaveChangesAsync();

		_model.BannedUserId = user.Id;

		var result = await _model.OnGet();

		Assert.IsInstanceOfType(result, typeof(PageResult));
		Assert.AreEqual("BannedUser", _model.UserName);
		Assert.IsNotNull(_model.BannedUntil);
		Assert.IsTrue(Math.Abs((_model.BannedUntil.Value - bannedUntil).TotalMilliseconds) < 1000);
	}

	[TestMethod]
	public async Task OnGet_UserNotBanned_HandlesCorrectly()
	{
		var user = _db.AddUser("UnbannedUser").Entity;
		await _db.SaveChangesAsync();

		_model.BannedUserId = user.Id;

		var result = await _model.OnGet();

		Assert.IsInstanceOfType(result, typeof(PageResult));
		Assert.AreEqual("UnbannedUser", _model.UserName);
		Assert.IsNull(_model.BannedUntil);
	}

	[TestMethod]
	public void AllowsAnonymousAttribute() => AssertAllowsAnonymousUsers(typeof(BannedModel));
}
