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
		CreateUsersWithMoodAvatars();
		await _db.SaveChangesAsync();

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
		CreateUsersWithMoodAvatars();
		await _db.SaveChangesAsync();
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
		CreateUsersWithMoodAvatars();
		await _db.SaveChangesAsync();
		_model.UserName = userName;

		await _model.OnGet();

		Assert.AreEqual(2, _model.MoodyUsers.Count);
	}

	[TestMethod]
	public async Task OnGet_UsersWithoutMoodAvatars_AreExcluded()
	{
		CreateUsersWithAndWithoutMoodAvatars();
		await _db.SaveChangesAsync();

		await _model.OnGet();

		Assert.AreEqual(1, _model.MoodyUsers.Count);
		Assert.AreEqual("UserWithMood", _model.MoodyUsers.First().UserName);
	}

	[TestMethod]
	public async Task OnGet_UsersWithoutMoodAvatarPermission_AreExcluded()
	{
		var roleWithPermission = CreateRoleWithMoodAvatarPermission();
		var roleWithoutPermission = CreateRoleWithoutMoodAvatarPermission();

		var userWithPermission = CreateUser("UserWithPermission", "https://example.com/moods/");
		var userWithoutPermission = CreateUser("UserWithoutPermission", "https://example.com/moods2/");

		AssignUserToRole(userWithPermission, roleWithPermission);
		AssignUserToRole(userWithoutPermission, roleWithoutPermission);

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

	private void CreateUsersWithMoodAvatars()
	{
		var role = CreateRoleWithMoodAvatarPermission();

		var user1 = CreateUser("User1", "https://example.com/moods1/");
		var user2 = CreateUser("User2", "https://example.com/moods2/");

		AssignUserToRole(user1, role);
		AssignUserToRole(user2, role);
	}

	private void CreateUsersWithAndWithoutMoodAvatars()
	{
		var role = CreateRoleWithMoodAvatarPermission();

		var userWithMood = CreateUser("UserWithMood", "https://example.com/moods/");
		var userWithoutMood = CreateUser("UserWithoutMood", null);

		AssignUserToRole(userWithMood, role);
		AssignUserToRole(userWithoutMood, role);
	}

	private User CreateUser(string userName, string? moodAvatarUrl)
		=> _db.Users.Add(new User
		{
			UserName = userName,
			NormalizedUserName = userName.ToUpperInvariant(),
			Email = $"{userName.ToLowerInvariant()}@example.com",
			NormalizedEmail = $"{userName.ToUpperInvariant()}@EXAMPLE.COM",
			MoodAvatarUrlBase = moodAvatarUrl
		}).Entity;

	private Role CreateRoleWithMoodAvatarPermission()
	{
		var role = _db.Roles.Add(new Role
		{
			Name = "MoodUser",
			NormalizedName = "MOODUSER"
		}).Entity;

		role.RolePermission.Add(new RolePermission
		{
			Role = role,
			PermissionId = PermissionTo.UseMoodAvatars
		});

		return role;
	}

	private Role CreateRoleWithoutMoodAvatarPermission()
	{
		var role = _db.Roles.Add(new Role
		{
			Name = "RegularUser",
			NormalizedName = "REGULARUSER"
		}).Entity;

		role.RolePermission.Add(new RolePermission
		{
			Role = role,
			PermissionId = PermissionTo.SubmitMovies
		});

		return role;
	}

	private void AssignUserToRole(User user, Role role)
		=> _db.UserRoles.Add(new UserRole { User = user, Role = role });
}
