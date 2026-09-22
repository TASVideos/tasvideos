using Microsoft.AspNetCore.Identity;
using TASVideos.Data.Entity;
using TASVideos.Data.Entity.Forum;

namespace TASVideos.IntegrationTests;

[TestClass]
#pragma warning disable CA1001
public class PermissionTests
#pragma warning restore CA1001
{
	private TASVideosWebApplicationFactory _factory = null!;
	private HttpClient _userClient = null!;
	private HttpClient _modClient = null!;

	[TestInitialize]
	public void Setup()
	{
		_factory = new TASVideosWebApplicationFactory(true);
		_userClient = _factory.CreateClientWithFollowRedirects();
		_modClient = _factory.CreateClientWithFollowRedirects();

		var passwordHasher = _factory.Services.GetRequiredService<IPasswordHasher<User>>();

		_factory.SeedDatabase(db =>
		{
			var roleTopicCreator = db.Roles.Add(new Role
			{
				Id = 1,
				Name = "TopicCreator",
				RolePermission = [
					new() { PermissionId = PermissionTo.CreateForumTopics },
					new() { PermissionId = PermissionTo.CreateForumPosts }
				]
			}).Entity;

			var roleModerator = db.Roles.Add(new Role
			{
				Id = 2,
				Name = "Moderator",
				RolePermission = [
					new() { PermissionId = PermissionTo.EditUsers },
					new() { PermissionId = PermissionTo.EditRoles },
					new() { PermissionId = PermissionTo.CreateForumTopics, CanAssign = true },
				]
			}).Entity;

			var userRegular = db.Users.Add(new User
			{
				Id = 1,
				UserName = "TestUser",
				NormalizedUserName = "TESTUSER",
				Email = "test@example.com",
				NormalizedEmail = "TEST@EXAMPLE.COM",
				SecurityStamp = Guid.NewGuid().ToString()
			}).Entity;

			var userModerator = db.Users.Add(new User
			{
				Id = 2,
				UserName = "TestModerator",
				NormalizedUserName = "TESTMODERATOR",
				Email = "moderator@example.com",
				NormalizedEmail = "MODERATOR@EXAMPLE.COM",
				SecurityStamp = Guid.NewGuid().ToString()
			}).Entity;

			userRegular.PasswordHash = passwordHasher.HashPassword(userRegular, "TestPassword");
			userModerator.PasswordHash = passwordHasher.HashPassword(userModerator, "TestPassword");

			db.UserRoles.Add(new() { User = userRegular, Role = roleTopicCreator });
			db.UserRoles.Add(new() { User = userModerator, Role = roleModerator });

			var forum = db.Forums.Add(new Forum
			{
				Id = 1,
				Name = "TestForum",
				Category = new() { Title = "TestCategory" }
			}).Entity;
		});
	}

	[TestCleanup]
	public void Cleanup()
	{
		_userClient.Dispose();
		_modClient.Dispose();
		_factory.Dispose();
	}

	private static async Task<string> GetToken(HttpClient client, string requestUri)
	{
		var response = await client.GetAsync(requestUri);
		var token = await response.QuerySelectorAsync("input[name='__RequestVerificationToken']");
		return token?.GetAttribute("value") ?? "";
	}

	private static async Task<bool> Login(HttpClient client, string userName, string password)
	{
		var token = await GetToken(client, "/Account/Login");
		var response = await client.PostAsync("/Account/Login", new FormUrlEncodedContent(new Dictionary<string, string>
		{
			{ "UserName", userName },
			{ "Password", password },
			{ "__RequestVerificationToken", token }
		}));
		return response.IsSuccessStatusCode;
	}

	private static async Task<bool> CreateTopic(HttpClient client, string title, string post)
	{
		var token = await GetToken(client, "/Forum/Topics/Create/1");
		var response = await client.PostAsync("/Forum/Topics/Create/1", new FormUrlEncodedContent(new Dictionary<string, string>
		{
			{ "Title", title },
			{ "Post", post },
			{ "__RequestVerificationToken", token }
		}));
		return response.IsSuccessStatusCode;
	}

	[TestMethod]
	public async Task RoleUnassigned_CanNoLongerCreateTopics()
	{
		// make post to ensure the user has the permissions
		var loginSuccess = await Login(_userClient, "TestUser", "TestPassword");
		Assert.IsTrue(loginSuccess);
		var createSuccess = await CreateTopic(_userClient, "TestTopic", "TestPost");
		Assert.IsTrue(createSuccess);

		// remove the role from the user
		var modLoginSuccess = await Login(_modClient, "TestModerator", "TestPassword");
		Assert.IsTrue(modLoginSuccess);
		var removeRoleToken = await GetToken(_modClient, "/Users/Edit/1");
		var removeRoleResponse = await _modClient.PostAsync("/Users/Edit/1", new FormUrlEncodedContent(new Dictionary<string, string>
		{
			// no UserToEdit.SelectedRoles
			{ "__RequestVerificationToken", removeRoleToken },
		}));
		Assert.IsTrue(removeRoleResponse.IsSuccessStatusCode);

		// attempt to create topic, should fail
		var createSuccessAfterRoleRemoved = await CreateTopic(_userClient, "TestTopicAfterRoleRemoved", "TestPostAfterRoleRemoved");
		Assert.IsFalse(createSuccessAfterRoleRemoved);
	}

	[TestMethod]
	public async Task PermissionRemovedFromRole_CanNoLongerCreateTopics()
	{
		// make post to ensure the user has the permissions
		var loginSuccess = await Login(_userClient, "TestUser", "TestPassword");
		Assert.IsTrue(loginSuccess);
		var createSuccess = await CreateTopic(_userClient, "TestTopic", "TestPost");
		Assert.IsTrue(createSuccess);

		// remove the permission from the role
		var modLoginSuccess = await Login(_modClient, "TestModerator", "TestPassword");
		Assert.IsTrue(modLoginSuccess);
		var removePermissionToken = await GetToken(_modClient, "/Roles/AddEdit/1");
		var removePermissionResponse = await _modClient.PostAsync("/Roles/AddEdit/1", new FormUrlEncodedContent(new Dictionary<string, string>
		{
			{ "Role.SelectedPermissions", ((int)PermissionTo.CreateForumPosts).ToString() }, // this removes CreateForumTopics but leaves CreateForumPosts because roles need at least one permission
			{ "__RequestVerificationToken", removePermissionToken },
		}));
		Assert.IsTrue(removePermissionResponse.IsSuccessStatusCode);

		// attempt to create topic, should fail
		var createSuccessAfterRoleRemoved = await CreateTopic(_userClient, "TestTopicAfterRoleRemoved", "TestPostAfterRoleRemoved");
		Assert.IsFalse(createSuccessAfterRoleRemoved);
	}
}
