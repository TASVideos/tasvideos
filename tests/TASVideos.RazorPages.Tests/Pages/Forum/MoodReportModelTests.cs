using TASVideos.Pages.Forum;
using TASVideos.Tests.Base;

namespace TASVideos.RazorPages.Tests.Pages.Forum;

[TestClass]
public class MoodReportModelTests : TestDbBase
{
	private readonly MoodReportModel _model;

	public MoodReportModelTests()
	{
		_model = new MoodReportModel(_db);
	}

	[TestMethod]
	public async Task OnGet_NoUserName_ReturnsAllUsersWithMoodAvatars()
	{
		await CreateUsersWithMoodAvatars();

		await _model.OnGet();

		Assert.AreEqual(2, _model.MoodyUsers.Count);
		Assert.IsTrue(_model.MoodyUsers.Any(u => u.UserName == "User1"));
		Assert.IsTrue(_model.MoodyUsers.Any(u => u.UserName == "User2"));
		Assert.AreEqual("https://example.com/moods1/", _model.MoodyUsers.First(u => u.UserName == "User1").MoodAvatarUrl);
		Assert.AreEqual("https://example.com/moods2/", _model.MoodyUsers.First(u => u.UserName == "User2").MoodAvatarUrl);
	}

	[TestMethod]
	public async Task OnGet_WithUserName_ReturnsSpecificUser()
	{
		await CreateUsersWithMoodAvatars();
		_model.UserName = "User1";

		await _model.OnGet();

		Assert.AreEqual(1, _model.MoodyUsers.Count);
		Assert.AreEqual("User1", _model.MoodyUsers.First().UserName);
		Assert.AreEqual("https://example.com/moods1/", _model.MoodyUsers.First().MoodAvatarUrl);
	}

	[TestMethod]
	public async Task OnGet_WithNonExistentUserName_ReturnsEmpty()
	{
		_model.UserName = "NonExistentUser";
		await _model.OnGet();
		Assert.AreEqual(0, _model.MoodyUsers.Count);
	}

	[TestMethod]
	[DataRow(null)]
	[DataRow("")]
	[DataRow("  ")]
	public async Task OnGet_WitNullOrWhitespaceUserName_ReturnsAllUsers(string userName)
	{
		await CreateUsersWithMoodAvatars();
		_model.UserName = userName;

		await _model.OnGet();

		Assert.AreEqual(2, _model.MoodyUsers.Count);
	}

	[TestMethod]
	public async Task OnGet_UsersWithoutMoodAvatars_AreExcluded()
	{
		var role = _db.AddRoleWithPermission(PermissionTo.UseMoodAvatars).Entity;

		var userWithMood = _db.AddUser("UserWithMood").Entity;
		userWithMood.MoodAvatarUrlBase = "https://example.com/moods/";
		var userWithoutMood = _db.AddUser("UserWithoutMood").Entity;

		_db.AssignUserToRole(userWithMood, role);
		_db.AssignUserToRole(userWithoutMood, role);
		await _db.SaveChangesAsync();

		await _model.OnGet();

		Assert.AreEqual(1, _model.MoodyUsers.Count);
		Assert.AreEqual("UserWithMood", _model.MoodyUsers.First().UserName);
	}

	[TestMethod]
	public async Task OnGet_UsersWithoutMoodAvatarPermission_AreExcluded()
	{
		var roleWithPermission = _db.AddRoleWithPermission(PermissionTo.UseMoodAvatars).Entity;
		var userWithPermission = _db.AddUser("UserWithPermission").Entity;
		userWithPermission.MoodAvatarUrlBase = "https://example.com/moods/";
		_db.AssignUserToRole(userWithPermission, roleWithPermission);

		var userWithoutPermission = _db.AddUserWithRole("UserWithoutPermission").Entity;
		userWithoutPermission.MoodAvatarUrlBase = "https://example.com/moods2/";

		await _db.SaveChangesAsync();

		await _model.OnGet();

		Assert.AreEqual(1, _model.MoodyUsers.Count);
		Assert.AreEqual("UserWithPermission", _model.MoodyUsers.First().UserName);
	}

	[TestMethod]
	public async Task OnGet_NoMoodyUsers_ReturnsEmpty()
	{
		await _model.OnGet();
		Assert.AreEqual(0, _model.MoodyUsers.Count);
	}

	[TestMethod]
	public void AllowsAnonymousAttribute() => AssertAllowsAnonymousUsers(typeof(MoodReportModel));

	private async Task CreateUsersWithMoodAvatars()
	{
		var role = _db.AddRoleWithPermission(PermissionTo.UseMoodAvatars).Entity;

		var user1 = _db.AddUser("User1").Entity;
		user1.MoodAvatarUrlBase = "https://example.com/moods1/";
		var user2 = _db.AddUser("User2").Entity;
		user2.MoodAvatarUrlBase = "https://example.com/moods2/";

		_db.AssignUserToRole(user1, role);
		_db.AssignUserToRole(user2, role);
		await _db.SaveChangesAsync();
	}
}
