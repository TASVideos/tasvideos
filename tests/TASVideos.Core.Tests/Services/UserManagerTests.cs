using System.Security.Claims;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using TASVideos.Core.Services.Wiki;
using TASVideos.Data.Entity;
using TASVideos.Data.Entity.Forum;
using TASVideos.Data.Entity.Game;

namespace TASVideos.Core.Tests.Services;

[TestClass]
public sealed class UserManagerTests : TestDbBase, IDisposable
{
	private readonly ITASVideoAgent _tasVideoAgent;
	private readonly IWikiPages _wikiPages;

	private readonly UserManager _userManager;

	public UserManagerTests()
	{
		_tasVideoAgent = Substitute.For<ITASVideoAgent>();
		_wikiPages = Substitute.For<IWikiPages>();
		_userManager = new UserManager(
			_db,
			new TestCache(),
			Substitute.For<IPointsService>(),
			_tasVideoAgent,
			_wikiPages,
			Substitute.For<IUserStore<User>>(),
			Substitute.For<IOptions<IdentityOptions>>(),
			Substitute.For<IPasswordHasher<User>>(),
			Substitute.For<IEnumerable<IUserValidator<User>>>(),
			Substitute.For<IEnumerable<IPasswordValidator<User>>>(),
			Substitute.For<ILookupNormalizer>(),
			new IdentityErrorDescriber(),
			Substitute.For<IServiceProvider>(),
			Substitute.For<ILogger<UserManager<User>>>());
	}

	[TestMethod]
	public async Task GetRequiredUser_UserDoesNotExist_Throws()
	{
		var user = new ClaimsPrincipal();
		await Assert.ThrowsExactlyAsync<InvalidOperationException>(() => _userManager.GetRequiredUser(user));
	}

	[TestMethod]
	public async Task GetUserPermissionsById_UserBanned_ReturnsNoRoles()
	{
		var user = _db.AddUser("user");
		user.Entity.BannedUntil = DateTime.UtcNow.AddYears(1);
		var role1 = _db.Roles.Add(new Role { Name = "Role1" });
		var role2 = _db.Roles.Add(new Role { Name = "Role2" });
		_db.RolePermission.Add(new RolePermission { PermissionId = PermissionTo.Unpublish, Role = role1.Entity });
		_db.RolePermission.Add(new RolePermission { PermissionId = PermissionTo.AssignRoles, Role = role1.Entity });
		_db.RolePermission.Add(new RolePermission { PermissionId = PermissionTo.Unpublish, Role = role2.Entity });
		_db.RolePermission.Add(new RolePermission { PermissionId = PermissionTo.EditUsers, Role = role2.Entity });
		_db.UserRoles.Add(new UserRole { User = user.Entity, Role = role1.Entity });
		_db.UserRoles.Add(new UserRole { User = user.Entity, Role = role2.Entity });
		await _db.SaveChangesAsync();

		var actual = await _userManager.GetUserPermissionsById(user.Entity.Id);
		Assert.AreEqual(0, actual.Count);
	}

	[TestMethod]
	public async Task GetUserPermissionsById_ReturnsDistinctList()
	{
		var user = _db.AddUser("user");
		var role1 = _db.Roles.Add(new Role { Name = "r1" });
		var role2 = _db.Roles.Add(new Role { Name = "r2" });
		_db.RolePermission.Add(new RolePermission { PermissionId = PermissionTo.Unpublish, Role = role1.Entity });
		_db.RolePermission.Add(new RolePermission { PermissionId = PermissionTo.AssignRoles, Role = role1.Entity });
		_db.RolePermission.Add(new RolePermission { PermissionId = PermissionTo.Unpublish, Role = role2.Entity });
		_db.RolePermission.Add(new RolePermission { PermissionId = PermissionTo.EditUsers, Role = role2.Entity });
		_db.UserRoles.Add(new UserRole { User = user.Entity, Role = role1.Entity });
		_db.UserRoles.Add(new UserRole { User = user.Entity, Role = role2.Entity });
		await _db.SaveChangesAsync();

		var actual = await _userManager.GetUserPermissionsById(user.Entity.Id);
		Assert.AreEqual(3, actual.Count);
	}

	#region AddStandardRoles()

	[TestMethod]
	public async Task AddStandardRoles_ThrowsIfUserDoesNotExist()
	{
		await Assert.ThrowsExactlyAsync<InvalidOperationException>(() => _userManager.AddStandardRoles(-1));
	}

	[TestMethod]
	public async Task AddStandardRoles_UserDoesNotHaveRole_AddsRole()
	{
		const int userId = 1;
		_db.AddUser(userId);
		var role1 = _db.Roles.Add(new Role { Name = "r1", IsDefault = true });
		var role2 = _db.Roles.Add(new Role { Name = "r2", IsDefault = true });
		var role3 = _db.Roles.Add(new Role { Name = "r3" });
		await _db.SaveChangesAsync();

		await _userManager.AddStandardRoles(userId);
		Assert.AreEqual(1, _db.UserRoles.Count(ur => ur.Role!.Name == role1.Entity.Name));
		Assert.AreEqual(1, _db.UserRoles.Count(ur => ur.Role!.Name == role2.Entity.Name));
		Assert.AreEqual(0, _db.UserRoles.Count(ur => ur.Role!.Name == role3.Entity.Name));
	}

	[TestMethod]
	public async Task AddStandardRoles_UserHasRole_DoesNotAddRole()
	{
		const int userId = 1;
		_db.AddUser(userId);
		var role1 = _db.Roles.Add(new Role { Name = "r1", IsDefault = true });
		_db.UserRoles.Add(new UserRole { UserId = userId, Role = role1.Entity });
		await _db.SaveChangesAsync();

		await _userManager.AddStandardRoles(userId);
		Assert.AreEqual(1, _db.UserRoles.Count());
	}

	#endregion

	#region AssignAutoAssignableRolesByPost()

	[TestMethod]
	public async Task AssignAutoAssignableRolesByPost_NoPosts_DoesNothing()
	{
		const int currentUserId = 1;
		_db.AddUser(currentUserId);
		const int anotherUserId = 2;
		_db.AddUser(anotherUserId);
		_db.Roles.Add(new Role { Name = "r", AutoAssignPostCount = 1 });
		var topic = _db.AddTopic().Entity;
		_db.ForumPosts.Add(new ForumPost { PosterId = anotherUserId, Topic = topic, Forum = topic.Forum });
		await _db.SaveChangesAsync();

		await _userManager.AssignAutoAssignableRolesByPost(currentUserId);
		Assert.AreEqual(0, _db.UserRoles.Count());
	}

	[TestMethod]
	public async Task AssignAutoAssignableRolesByPost_NotEnoughPosts_DoesNothing()
	{
		const int userId = 1;
		_db.AddUser(userId);
		var topic = _db.AddTopic().Entity;
		_db.ForumPosts.Add(new ForumPost { PosterId = userId, Topic = topic, Forum = topic.Forum });
		_db.Roles.Add(new Role { Name = "r", AutoAssignPostCount = 2 });
		await _db.SaveChangesAsync();

		await _userManager.AssignAutoAssignableRolesByPost(userId);
		Assert.AreEqual(0, _db.UserRoles.Count());
		_ = _tasVideoAgent.DidNotReceive().SendAutoAssignedRole(userId, Arg.Any<string>());
	}

	[TestMethod]
	public async Task AssignAutoAssignableRolesByPost_EnoughPosts_AddsRole()
	{
		const int userId = 1;
		_db.AddUser(userId);
		var topic = _db.AddTopic().Entity;
		_db.ForumPosts.Add(new ForumPost { PosterId = userId, Topic = topic, Forum = topic.Forum });
		var role = _db.Roles.Add(new Role { Name = "r", AutoAssignPostCount = 1 });
		_db.RolePermission.Add(new RolePermission { Role = role.Entity, PermissionId = PermissionTo.CreateForumTopics });
		await _db.SaveChangesAsync();

		await _userManager.AssignAutoAssignableRolesByPost(userId);
		Assert.AreEqual(1, _db.UserRoles.Count(ur => ur.UserId == userId));
		_ = _tasVideoAgent.Received().SendAutoAssignedRole(userId, Arg.Any<string>());
	}

	[TestMethod]
	public async Task AssignAutoAssignableRolesByPost_AlreadyHasRole_AddsNothing()
	{
		const int userId = 1;
		_db.AddUser(userId);
		var topic = _db.AddTopic().Entity;
		_db.ForumPosts.Add(new ForumPost { PosterId = userId, Topic = topic, Forum = topic.Forum });
		var role = _db.Roles.Add(new Role { Name = "r", AutoAssignPostCount = 1 });
		_db.RolePermission.Add(new RolePermission { Role = role.Entity, PermissionId = PermissionTo.CreateForumTopics });
		_db.UserRoles.Add(new UserRole { UserId = userId, Role = role.Entity });
		await _db.SaveChangesAsync();

		await _userManager.AssignAutoAssignableRolesByPost(userId);
		Assert.AreEqual(1, _db.UserRoles.Count(ur => ur.UserId == userId));
		_ = _tasVideoAgent.DidNotReceive().SendAutoAssignedRole(userId, Arg.Any<string>());
	}

	[TestMethod]
	public async Task AssignAutoAssignableRolesByPost_AlreadyHasAllPermissionsFromRole_DoesNotAddRole()
	{
		const int userId = 1;
		_db.AddUser(userId);
		var topic = _db.AddTopic().Entity;
		_db.ForumPosts.Add(new ForumPost { PosterId = userId, Topic = topic, Forum = topic.Forum });
		var role1 = _db.Roles.Add(new Role { Name = "r1" });
		var role2 = _db.Roles.Add(new Role { Name = "r2", AutoAssignPostCount = 1 });
		var role3 = _db.Roles.Add(new Role { Name = "r3", AutoAssignPostCount = 1 });
		_db.RolePermission.Add(new RolePermission { Role = role1.Entity, PermissionId = PermissionTo.CreateForumTopics });
		_db.RolePermission.Add(new RolePermission { Role = role2.Entity, PermissionId = PermissionTo.CreateForumTopics });
		_db.RolePermission.Add(new RolePermission { Role = role3.Entity, PermissionId = PermissionTo.CatalogMovies });
		_db.UserRoles.Add(new UserRole { UserId = userId, Role = role1.Entity });
		await _db.SaveChangesAsync();

		await _userManager.AssignAutoAssignableRolesByPost(userId);
		Assert.AreEqual(1, _db.UserRoles.Count(ur => ur.UserId == userId && ur.RoleId == role1.Entity.Id));
		Assert.AreEqual(0, _db.UserRoles.Count(ur => ur.UserId == userId && ur.RoleId == role2.Entity.Id));
		Assert.AreEqual(1, _db.UserRoles.Count(ur => ur.UserId == userId && ur.RoleId == role3.Entity.Id));
		_ = _tasVideoAgent.DidNotReceive().SendAutoAssignedRole(userId, role2.Entity.Name);
		_ = _tasVideoAgent.Received().SendAutoAssignedRole(userId, role3.Entity.Name);
	}

	#endregion

	#region AssignAutoAssignableRolesByPublication()

	[TestMethod]
	public async Task AssignAutoAssignableRolesByPublication_NotUsers_DoesNothing()
	{
		await _userManager.AssignAutoAssignableRolesByPublication([], "pub title");
		Assert.AreEqual(0, _db.UserRoles.Count());
		_ = _tasVideoAgent.DidNotReceive().SendPublishedAuthorRole(Arg.Any<int>(), Arg.Any<string>(), Arg.Any<string>());
	}

	[TestMethod]
	public async Task AssignAutoAssignableRolesByPublication_ArePublishedAuthors_AddsRoleIfPublishedAuthor()
	{
		const int publishedAuthor1Id = 1;
		const int publishedAuthor2Id = 2;
		const int noAuthorId = 3;
		const string pubTitle = "some title";
		_db.AddUser(publishedAuthor1Id);
		_db.AddUser(publishedAuthor2Id);
		_db.AddUser(noAuthorId);
		var pub = _db.AddPublication().Entity;
		_db.PublicationAuthors.Add(new PublicationAuthor { Publication = pub, UserId = publishedAuthor1Id });
		_db.PublicationAuthors.Add(new PublicationAuthor { Publication = pub, UserId = publishedAuthor2Id });
		var role = _db.Roles.Add(new Role { Name = "r", AutoAssignPublications = true });
		_db.RolePermission.Add(new RolePermission { Role = role.Entity, PermissionId = PermissionTo.CatalogMovies });
		await _db.SaveChangesAsync();

		await _userManager.AssignAutoAssignableRolesByPublication([publishedAuthor1Id, publishedAuthor2Id, noAuthorId], pubTitle);
		Assert.AreEqual(1, _db.UserRoles.Count(ur => ur.UserId == publishedAuthor1Id && ur.RoleId == role.Entity.Id));
		Assert.AreEqual(1, _db.UserRoles.Count(ur => ur.UserId == publishedAuthor2Id && ur.RoleId == role.Entity.Id));
		Assert.AreEqual(0, _db.UserRoles.Count(ur => ur.UserId == noAuthorId && ur.RoleId == role.Entity.Id));
		_ = _tasVideoAgent.Received().SendPublishedAuthorRole(publishedAuthor1Id, role.Entity.Name, pubTitle);
		_ = _tasVideoAgent.Received().SendPublishedAuthorRole(publishedAuthor2Id, Arg.Any<string>(), pubTitle);
		_ = _tasVideoAgent.DidNotReceive().SendPublishedAuthorRole(noAuthorId, Arg.Any<string>(), Arg.Any<string>());
	}

	[TestMethod]
	public async Task AssignAutoAssignableRolesByPublication_UserAlreadyHasAllPermissions_DoesNotAddRole()
	{
		const int userId = 1;
		_db.AddUser(1);
		var pub = _db.AddPublication().Entity;
		_db.PublicationAuthors.Add(new PublicationAuthor { Publication = pub, UserId = userId });
		var role1 = _db.Roles.Add(new Role { Name = "r1", AutoAssignPublications = true });
		var role2 = _db.Roles.Add(new Role { Name = "r2" });
		_db.UserRoles.Add(new UserRole { UserId = userId, Role = role2.Entity });
		await _userManager.AssignAutoAssignableRolesByPublication([userId], "some title");
		Assert.AreEqual(0, _db.UserRoles.Count(ur => ur.UserId == userId && ur.RoleId == role1.Entity.Id));
		Assert.AreEqual(1, _db.UserRoles.Count(ur => ur.UserId == userId && ur.RoleId == role2.Entity.Id));
		_ = _tasVideoAgent.DidNotReceive().SendPublishedAuthorRole(userId, Arg.Any<string>(), Arg.Any<string>());
	}

	[TestMethod]
	public async Task AssignAutoAssignableRolesByPublication_BannedUserAlreadyHasRole_DoesNotAddRole()
	{
		const int userId = 1;
		var user = _db.AddUser(userId).Entity;
		user.BannedUntil = DateTime.UtcNow.AddYears(100);
		_db.AddPublication(user);
		var role = _db.Roles.Add(new Role { Name = "r1", AutoAssignPublications = true });
		_db.RolePermission.Add(new RolePermission { Role = role.Entity, PermissionId = PermissionTo.CatalogMovies });
		_db.UserRoles.Add(new UserRole { UserId = userId, Role = role.Entity });
		await _db.SaveChangesAsync();

		await _userManager.AssignAutoAssignableRolesByPublication([userId], "some title");
		Assert.AreEqual(1, _db.UserRoles.Count(ur => ur.UserId == userId && ur.RoleId == role.Entity.Id));
		_ = _tasVideoAgent.DidNotReceive().SendPublishedAuthorRole(userId, Arg.Any<string>(), Arg.Any<string>());
	}

	#endregion

	[TestMethod]
	public async Task UserNameChanged()
	{
		const int userId = 1;
		const string oldName = "oldName";
		const string newName = "newName";
		var user = _db.AddUser(userId, newName);
		var system = _db.GameSystems.Add(new GameSystem { Code = "code" });
		var systemFrameRate = _db.GameSystemFrameRates.Add(new GameSystemFrameRate { FrameRate = 60 });
		var game = _db.Games.Add(new Game { DisplayName = "game" });
		var gameGoal = _db.GameGoals.Add(new GameGoal { DisplayName = "game", Game = game.Entity });
		var gameVersion = _db.GameVersions.Add(new GameVersion { Game = game.Entity });
		var sub1 = _db.Submissions.Add(new Submission { System = system.Entity, SystemFrameRate = systemFrameRate.Entity, Submitter = user.Entity });
		var sub2 = _db.Submissions.Add(new Submission { System = system.Entity, SystemFrameRate = systemFrameRate.Entity, Submitter = user.Entity });
		_db.SubmissionAuthors.Add(new SubmissionAuthor { UserId = userId, Submission = sub1.Entity });
		_db.SubmissionAuthors.Add(new SubmissionAuthor { UserId = userId, Submission = sub2.Entity });
		sub1.Entity.GenerateTitle();
		sub2.Entity.GenerateTitle();
		var publicationClass = _db.PublicationClasses.Add(new PublicationClass { Name = "class" });
		var pub1 = _db.Publications.Add(new Publication { System = system.Entity, SystemFrameRate = systemFrameRate.Entity, Game = game.Entity, GameGoal = gameGoal.Entity, GameVersion = gameVersion.Entity, PublicationClass = publicationClass.Entity, Submission = sub1.Entity, MovieFileName = "1" });
		var pub2 = _db.Publications.Add(new Publication { System = system.Entity, SystemFrameRate = systemFrameRate.Entity, Game = game.Entity, GameGoal = gameGoal.Entity, GameVersion = gameVersion.Entity, PublicationClass = publicationClass.Entity, Submission = sub2.Entity, MovieFileName = "2" });
		_db.PublicationAuthors.Add(new PublicationAuthor { UserId = userId, Publication = pub1.Entity });
		_db.PublicationAuthors.Add(new PublicationAuthor { UserId = userId, Publication = pub2.Entity });
		pub1.Entity.GenerateTitle();
		pub2.Entity.GenerateTitle();
		await _db.SaveChangesAsync();

		await _userManager.UserNameChanged(user.Entity, oldName);
		_ = _wikiPages.Received().MoveAll(LinkConstants.HomePages + oldName, LinkConstants.HomePages + newName);
		Assert.AreEqual(0, _db.Submissions.Count(s => s.Title.Contains(oldName)));
		Assert.AreEqual(2, _db.Submissions.Count(s => s.Title.Contains(newName)));
		Assert.AreEqual(0, _db.Publications.Count(s => s.Title.Contains(oldName)));
		Assert.AreEqual(2, _db.Publications.Count(s => s.Title.Contains(newName)));
	}

	[TestMethod]
	[DataRow("test", "test", true)]
	[DataRow("test", "doesNotExist", false)]
	public async Task Exists(string userToAdd, string userToLookup, bool expected)
	{
		_db.AddUser(userToAdd);
		await _db.SaveChangesAsync();

		var actual = await _userManager.Exists(userToLookup);
		Assert.AreEqual(expected, actual);
	}

	[TestMethod]
	public async Task PermaBanUser()
	{
		const int userId = 1;
		var user = _db.AddUser(userId);

		await _userManager.PermaBanUser(userId);
		Assert.IsTrue(user.Entity.IsBanned());
	}

	[TestMethod]
	[DataRow("OldUser", "NewUser", "OtherExistingUser", true)]
	[DataRow("User", "user", "OtherExistingUser", true)]
	[DataRow("User", "CoolUser", "CoolUser", false)]
	[DataRow("User", "coolUser", "CoolUser", false)]
	public async Task CanRenameUser(string oldUserName, string newUserName, string otherExistingUserName, bool expected)
	{
		_db.AddUser(oldUserName);
		_db.AddUser(otherExistingUserName);
		await _db.SaveChangesAsync();

		var actual = await _userManager.CanRenameUser(oldUserName, newUserName);
		Assert.AreEqual(expected, actual);
	}

	public void Dispose() => _userManager.Dispose();
}
