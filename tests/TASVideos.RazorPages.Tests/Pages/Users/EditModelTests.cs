using Microsoft.AspNetCore.Mvc.Rendering;
using TASVideos.Core.Services;
using TASVideos.Core.Services.ExternalMediaPublisher;
using TASVideos.Pages.Users;
using TASVideos.Services;
using TASVideos.Tests.Base;

namespace TASVideos.RazorPages.Tests.Pages.Users;

[TestClass]
public class EditModelTests : TestDbBase
{
	private readonly IRoleService _roleService;
	private readonly IExternalMediaPublisher _publisher;
	private readonly IUserMaintenanceLogger _userMaintenanceLogger;
	private readonly IUserManager _userManager;
	private readonly EditModel _model;

	public EditModelTests()
	{
		_roleService = Substitute.For<IRoleService>();
		_publisher = Substitute.For<IExternalMediaPublisher>();
		_userMaintenanceLogger = Substitute.For<IUserMaintenanceLogger>();
		_userManager = Substitute.For<IUserManager>();
		_model = new EditModel(_roleService, _db, _publisher, _userMaintenanceLogger, _userManager);
	}

	#region OnGet

	[TestMethod]
	public async Task OnGet_UserEditingSelf_RedirectsToProfile()
	{
		var user = _db.AddUser("TestUser").Entity;
		AddAuthenticatedUser(_model, user, [PermissionTo.EditUsers]);
		_model.Id = user.Id;

		var result = await _model.OnGet();

		Assert.IsInstanceOfType<RedirectToPageResult>(result);
		var redirect = (RedirectToPageResult)result;
		Assert.AreEqual("/Profile/Settings", redirect.PageName);
	}

	[TestMethod]
	public async Task OnGet_UserNotFound_ReturnsNotFound()
	{
		_model.Id = 999; // Non-existent user ID
		var result = await _model.OnGet();
		Assert.IsInstanceOfType<NotFoundResult>(result);
	}

	[TestMethod]
	public async Task OnGet_ValidUser_LoadsUserDataAndRoles()
	{
		var authenticatedUser = _db.AddUser("AuthenticatedUser").Entity;
		var targetUser = _db.AddUser("TargetUser").Entity;
		targetUser.TimeZoneId = "America/New_York";
		targetUser.From = "New York";
		targetUser.Email = "target@example.com";
		targetUser.EmailConfirmed = true;
		targetUser.Signature = "Test signature";
		targetUser.Avatar = "avatar.png";
		targetUser.MoodAvatarUrlBase = "mood.png";
		targetUser.UseRatings = true;
		targetUser.ModeratorComments = "Test comments";

		var role1 = _db.Roles.Add(new Role { Id = 1, Name = "Role1" });
		_db.Roles.Add(new Role { Id = 2, Name = "Role2" });
		_db.UserRoles.Add(new UserRole { User = targetUser, Role = role1.Entity });
		await _db.SaveChangesAsync();

		AddAuthenticatedUser(_model, authenticatedUser, [PermissionTo.EditUsers]);
		_model.Id = targetUser.Id;

		var availableRoles = new List<SelectListItem>
		{
			new() { Text = "Role1", Value = "1" },
			new() { Text = "Role2", Value = "2" }
		};
		_roleService.GetAllRolesUserCanAssign(authenticatedUser.Id, Arg.Any<IEnumerable<int>>())
			.Returns(availableRoles.Select(r => new AssignableRole(int.Parse(r.Value), r.Text, false)));

		var result = await _model.OnGet();

		Assert.IsInstanceOfType<PageResult>(result);
		Assert.AreEqual("TargetUser", _model.UserToEdit.UserName);
		Assert.AreEqual("America/New_York", _model.UserToEdit.TimeZone);
		Assert.AreEqual("New York", _model.UserToEdit.Location);
		Assert.AreEqual("target@example.com", _model.UserToEdit.Email);
		Assert.IsTrue(_model.UserToEdit.EmailConfirmed);
		Assert.AreEqual("Test signature", _model.UserToEdit.Signature);
		Assert.AreEqual("avatar.png", _model.UserToEdit.Avatar);
		Assert.AreEqual("mood.png", _model.UserToEdit.MoodAvatar);
		Assert.IsTrue(_model.UserToEdit.UseRatings);
		Assert.AreEqual("Test comments", _model.UserToEdit.ModeratorComments);
		Assert.AreEqual(1, _model.UserToEdit.SelectedRoles.Count);
		Assert.AreEqual(1, _model.UserToEdit.SelectedRoles[0]);
		Assert.AreEqual(2, _model.AvailableRoles.Count);
	}

	#endregion

	#region OnGetUnlock Tests

	[TestMethod]
	public async Task OnGetUnlock_UserNotFound_ReturnsNotFound()
	{
		_model.Id = 999; // Non-existent user ID
		var result = await _model.OnGetUnlock();
		Assert.IsInstanceOfType<NotFoundResult>(result);
	}

	[TestMethod]
	public async Task OnGetUnlock_ValidUser_UnlocksUserAndRedirects()
	{
		var authenticatedUser = _db.AddUser("AuthenticatedUser").Entity;
		var targetUser = _db.AddUser("TargetUser").Entity;
		targetUser.LockoutEnd = DateTime.UtcNow.AddHours(1);
		await _db.SaveChangesAsync();

		AddAuthenticatedUser(_model, authenticatedUser, [PermissionTo.EditUsers]);
		_model.Id = targetUser.Id;

		var result = await _model.OnGetUnlock();

		Assert.IsInstanceOfType<RedirectToPageResult>(result);

		// Verify user was unlocked
		var updatedUser = await _db.Users.FindAsync(targetUser.Id);
		Assert.IsNotNull(updatedUser);
		Assert.IsNull(updatedUser.LockoutEnd);
	}

	#endregion

	#region OnPost Tests

	[TestMethod]
	public async Task OnPost_InvalidModelState_ReturnsPageWithRoles()
	{
		var user = _db.AddUser("TestUser").Entity;
		AddAuthenticatedUser(_model, user, [PermissionTo.EditUsers]);
		_model.Id = user.Id;
		_model.UserToEdit = new EditModel.UserEditModel { UserName = "TestUser" };
		_model.ModelState.AddModelError("test", "Test error");

		var availableRoles = new List<AssignableRole>
		{
			new(1, "Role1", false)
		};
		_roleService.GetAllRolesUserCanAssign(user.Id, Arg.Any<IEnumerable<int>>())
			.Returns(availableRoles);

		var result = await _model.OnPost();

		Assert.IsInstanceOfType<PageResult>(result);
		Assert.IsFalse(_model.ModelState.IsValid);
		Assert.AreEqual(1, _model.AvailableRoles.Count);
	}

	[TestMethod]
	public async Task OnPost_UserNotFound_ReturnsNotFound()
	{
		var user = _db.AddUser("TestUser").Entity;
		AddAuthenticatedUser(_model, user, [PermissionTo.EditUsers]);
		_model.Id = 999; // Non-existent user ID
		_model.UserToEdit = new EditModel.UserEditModel { UserName = "TestUser" };

		_roleService.GetAllRolesUserCanAssign(user.Id, Arg.Any<IEnumerable<int>>())
			.Returns([]);

		var result = await _model.OnPost();

		Assert.IsInstanceOfType<NotFoundResult>(result);
	}

	[TestMethod]
	public async Task OnPost_UnauthorizedRoleAssignment_ReturnsAccessDenied()
	{
		var authenticatedUser = _db.AddUser("AuthenticatedUser").Entity;
		var targetUser = _db.AddUser("TargetUser").Entity;
		await _db.SaveChangesAsync();

		AddAuthenticatedUser(_model, authenticatedUser, [PermissionTo.EditUsers]);
		_model.Id = targetUser.Id;
		_model.UserToEdit = new EditModel.UserEditModel
		{
			UserName = "TargetUser",
			SelectedRoles = [999] // Role user cannot assign
		};

		_roleService.GetAllRolesUserCanAssign(authenticatedUser.Id, Arg.Any<IEnumerable<int>>())
			.Returns([new(1, "Role1", false)]);

		var result = await _model.OnPost();

		AssertAccessDenied(result);
	}

	[TestMethod]
	public async Task OnPost_ValidUserUpdate_UpdatesUserAndRedirects()
	{
		var authenticatedUser = _db.AddUser("AuthenticatedUser").Entity;
		var targetUser = _db.AddUser("TargetUser").Entity;
		_db.Roles.Add(new Role { Id = 1, Name = "Role1" });
		await _db.SaveChangesAsync();

		AddAuthenticatedUser(_model, authenticatedUser, [PermissionTo.EditUsers]);
		_model.Id = targetUser.Id;
		_model.UserToEdit = new EditModel.UserEditModel
		{
			UserName = "TargetUser",
			TimeZone = "America/Los_Angeles",
			Location = "California",
			Signature = "New signature",
			Avatar = "NewAvatar.png",
			MoodAvatar = "NewMood.png",
			UseRatings = true,
			ModeratorComments = "Updated comments",
			SelectedRoles = [1]
		};

		_roleService.GetAllRolesUserCanAssign(authenticatedUser.Id, Arg.Any<IEnumerable<int>>())
			.Returns([new(1, "Role1", false)]);

		var result = await _model.OnPost();

		Assert.IsInstanceOfType<RedirectToPageResult>(result);
		var redirect = (RedirectToPageResult)result;
		Assert.AreEqual("List", redirect.PageName);

		// Verify user was updated
		var updatedUser = await _db.Users.FindAsync(targetUser.Id);
		Assert.IsNotNull(updatedUser);
		Assert.AreEqual("America/Los_Angeles", updatedUser.TimeZoneId);
		Assert.AreEqual("California", updatedUser.From);
		Assert.AreEqual("New signature", updatedUser.Signature);
		Assert.AreEqual("NewAvatar.png", updatedUser.Avatar);
		Assert.AreEqual("NewMood.png", updatedUser.MoodAvatarUrlBase);
		Assert.IsTrue(updatedUser.UseRatings);
		Assert.AreEqual("Updated comments", updatedUser.ModeratorComments);

		// Verify role was assigned
		var userRole = _db.UserRoles.FirstOrDefault(ur => ur.UserId == targetUser.Id && ur.RoleId == 1);
		Assert.IsNotNull(userRole);
	}

	[TestMethod]
	public async Task OnPost_UsernameChange_UpdatesUsernameAndCallsUserManager()
	{
		var authenticatedUser = _db.AddUser("AuthenticatedUser").Entity;
		var targetUser = _db.AddUser("TargetUser").Entity;
		await _db.SaveChangesAsync();

		AddAuthenticatedUser(_model, authenticatedUser, [PermissionTo.EditUsers]);
		_model.Id = targetUser.Id;
		_model.UserToEdit = new EditModel.UserEditModel
		{
			UserName = "NewUserName",
			SelectedRoles = []
		};

		_roleService.GetAllRolesUserCanAssign(authenticatedUser.Id, Arg.Any<IEnumerable<int>>())
			.Returns([]);

		_userManager.CanRenameUser("TargetUser", "NewUserName").Returns(true);

		var result = await _model.OnPost();

		Assert.IsInstanceOfType<RedirectToPageResult>(result);
		var redirect = (RedirectToPageResult)result;
		Assert.AreEqual("/Users/Profile", redirect.PageName);
		Assert.AreEqual("NewUserName", redirect.RouteValues!["Name"]);

		// Verify username was changed
		var updatedUser = await _db.Users.FindAsync(targetUser.Id);
		Assert.IsNotNull(updatedUser);
		Assert.AreEqual("NewUserName", updatedUser.UserName);

		// Verify UserManager methods were called
		await _userManager.Received(1).CanRenameUser("TargetUser", "NewUserName");
		await _userManager.Received(1).UserNameChanged(Arg.Any<User>(), "TargetUser");
		await _userMaintenanceLogger.Received(1).Log(targetUser.Id, Arg.Any<string>(), authenticatedUser.Id);
	}

	[TestMethod]
	public async Task OnPost_CannotRenameUser_SkipsUsernameChange()
	{
		var authenticatedUser = _db.AddUser("AuthenticatedUser").Entity;
		var targetUser = _db.AddUser("TargetUser").Entity;
		await _db.SaveChangesAsync();

		AddAuthenticatedUser(_model, authenticatedUser, [PermissionTo.EditUsers]);
		_model.Id = targetUser.Id;
		_model.UserToEdit = new EditModel.UserEditModel
		{
			UserName = "NewUserName",
			SelectedRoles = []
		};

		_roleService.GetAllRolesUserCanAssign(authenticatedUser.Id, Arg.Any<IEnumerable<int>>())
			.Returns(new List<AssignableRole>());

		_userManager.CanRenameUser("TargetUser", "NewUserName").Returns(false);

		var result = await _model.OnPost();

		Assert.IsInstanceOfType<RedirectToPageResult>(result);

		// Verify username was not changed
		var updatedUser = await _db.Users.FindAsync(targetUser.Id);
		Assert.IsNotNull(updatedUser);
		Assert.AreEqual("TargetUser", updatedUser.UserName);

		// Verify UserManager.UserNameChanged was not called
		await _userManager.DidNotReceive().UserNameChanged(Arg.Any<User>(), Arg.Any<string>());
	}

	[TestMethod]
	public async Task OnPost_RoleChanges_LogsAndAnnounces()
	{
		var authenticatedUser = _db.AddUser("AuthenticatedUser").Entity;
		var targetUser = _db.AddUser("TargetUser").Entity;
		var role1 = _db.Roles.Add(new Role { Id = 1, Name = "OldRole" });
		_db.Roles.Add(new Role { Id = 2, Name = "NewRole" });
		_db.UserRoles.Add(new UserRole { User = targetUser, Role = role1.Entity });
		await _db.SaveChangesAsync();

		AddAuthenticatedUser(_model, authenticatedUser, [PermissionTo.EditUsers]);
		_model.Id = targetUser.Id;
		_model.UserToEdit = new EditModel.UserEditModel
		{
			UserName = "TargetUser",
			SelectedRoles = [2] // Remove role1, add role2
		};

		_roleService.GetAllRolesUserCanAssign(authenticatedUser.Id, Arg.Any<IEnumerable<int>>())
			.Returns(
			[
				new(1, "OldRole", false),
				new(2, "NewRole", false)
			]);

		var result = await _model.OnPost();

		Assert.IsInstanceOfType<RedirectToPageResult>(result);

		// Verify role changes
		Assert.AreEqual(0, _db.UserRoles.Count(ur => ur.UserId == targetUser.Id && ur.RoleId == 1));
		Assert.AreEqual(1, _db.UserRoles.Count(ur => ur.UserId == targetUser.Id && ur.RoleId == 2));
		await _userMaintenanceLogger.Received(1).Log(targetUser.Id, Arg.Any<string>(), authenticatedUser.Id);
	}

	[TestMethod]
	public async Task OnPost_BannedUntilChange_UpdatesBanStatus()
	{
		var authenticatedUser = _db.AddUser("AuthenticatedUser").Entity;
		var targetUser = _db.AddUser("TargetUser").Entity;
		await _db.SaveChangesAsync();

		AddAuthenticatedUser(_model, authenticatedUser, [PermissionTo.EditUsers]);
		_model.Id = targetUser.Id;
		var banDate = DateTime.UtcNow.AddDays(30);
		_model.UserToEdit = new EditModel.UserEditModel
		{
			UserName = "TargetUser",
			BannedUntil = banDate,
			SelectedRoles = []
		};

		_roleService.GetAllRolesUserCanAssign(authenticatedUser.Id, Arg.Any<IEnumerable<int>>())
			.Returns([]);

		var result = await _model.OnPost();

		Assert.IsInstanceOfType<RedirectToPageResult>(result);
		var updatedUser = await _db.Users.FindAsync(targetUser.Id);
		Assert.IsNotNull(updatedUser);
		Assert.AreEqual(banDate, updatedUser.BannedUntil);
	}

	[TestMethod]
	public async Task OnPost_SaveFailsOnFirstSave_ReturnsRedirectWithError()
	{
		var authenticatedUser = _db.AddUser("AuthenticatedUser").Entity;
		var targetUser = _db.AddUser("TargetUser").Entity;
		await _db.SaveChangesAsync();

		AddAuthenticatedUser(_model, authenticatedUser, [PermissionTo.EditUsers]);
		_model.Id = targetUser.Id;
		_model.UserToEdit = new EditModel.UserEditModel
		{
			UserName = "TargetUser",
			SelectedRoles = []
		};

		_roleService.GetAllRolesUserCanAssign(authenticatedUser.Id, Arg.Any<IEnumerable<int>>())
			.Returns([]);

		// Force the user to have an invalid state that would cause save to fail
		targetUser.UserName = ""; // This would cause validation to fail
		await _db.SaveChangesAsync(); // Save the invalid state

		var result = await _model.OnPost();

		Assert.IsInstanceOfType<RedirectToPageResult>(result);
		var redirect = (RedirectToPageResult)result;
		Assert.AreEqual("List", redirect.PageName);
	}

	[TestMethod]
	public async Task OnPost_NoRoleChanges_DoesNotLogOrAnnounce()
	{
		var authenticatedUser = _db.AddUser("AuthenticatedUser").Entity;
		var targetUser = _db.AddUser("TargetUser").Entity;
		var role = _db.Roles.Add(new Role { Id = 1, Name = "Role1" });
		_db.UserRoles.Add(new UserRole { User = targetUser, Role = role.Entity });
		await _db.SaveChangesAsync();

		AddAuthenticatedUser(_model, authenticatedUser, [PermissionTo.EditUsers]);
		_model.Id = targetUser.Id;
		_model.UserToEdit = new EditModel.UserEditModel
		{
			UserName = "TargetUser",
			SelectedRoles = [1] // Same role
		};

		_roleService.GetAllRolesUserCanAssign(authenticatedUser.Id, Arg.Any<IEnumerable<int>>())
			.Returns([new(1, "Role1", false)]);

		var result = await _model.OnPost();

		Assert.IsInstanceOfType<RedirectToPageResult>(result);

		// Verify no role change announcements were made
		await _publisher.DidNotReceive().Send(Arg.Any<Post>());
		await _userMaintenanceLogger.DidNotReceive().Log(targetUser.Id, Arg.Any<string>(), authenticatedUser.Id);
	}

	[TestMethod]
	public void RequiresPermission() => AssertHasPermission(typeof(EditModel), PermissionTo.EditUsers);

	#endregion
}
