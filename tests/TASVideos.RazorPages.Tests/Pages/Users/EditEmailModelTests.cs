using TASVideos.Core.Services;
using TASVideos.Core.Services.ExternalMediaPublisher;
using TASVideos.Pages.Users;
using TASVideos.Services;
using TASVideos.Tests.Base;

namespace TASVideos.RazorPages.Tests.Pages.Users;

[TestClass]
public class EditEmailModelTests : TestDbBase
{
	private readonly IExternalMediaPublisher _publisher;
	private readonly IUserMaintenanceLogger _userMaintenanceLogger;
	private readonly ISignInManager _signInManager;
	private readonly IUserManager _userManager;
	private readonly EditEmailModel _model;

	public EditEmailModelTests()
	{
		_publisher = Substitute.For<IExternalMediaPublisher>();
		_userMaintenanceLogger = Substitute.For<IUserMaintenanceLogger>();
		_signInManager = Substitute.For<ISignInManager>();
		_userManager = Substitute.For<IUserManager>();
		_model = new EditEmailModel(_db, _publisher, _userMaintenanceLogger, _signInManager, _userManager);
	}

	#region OnGet

	[TestMethod]
	public async Task OnGet_UserEditingSelf_RedirectsToProfileSettings()
	{
		var user = _db.AddUser("TestUser").Entity;
		AddAuthenticatedUser(_model, user, [PermissionTo.SeeEmails, PermissionTo.EditUsers]);
		_model.Id = user.Id;

		var result = await _model.OnGet();

		Assert.IsInstanceOfType<RedirectToPageResult>(result);
		var redirect = (RedirectToPageResult)result;
		Assert.AreEqual("/Profile/Settings", redirect.PageName);
	}

	[TestMethod]
	public async Task OnGet_UserNotFound_ReturnsNotFound()
	{
		_model.Id = 999;
		var result = await _model.OnGet();
		Assert.IsInstanceOfType<NotFoundResult>(result);
	}

	[TestMethod]
	public async Task OnGet_ValidUser_LoadsUserData()
	{
		var authenticatedUser = _db.AddUser("AuthenticatedUser").Entity;
		var targetUser = _db.AddUser("TargetUser").Entity;
		targetUser.Email = "target@example.com";
		targetUser.EmailConfirmed = true;
		await _db.SaveChangesAsync();

		AddAuthenticatedUser(_model, authenticatedUser, [PermissionTo.SeeEmails, PermissionTo.EditUsers]);
		_model.Id = targetUser.Id;

		var result = await _model.OnGet();

		Assert.IsInstanceOfType<PageResult>(result);
		Assert.AreEqual("TargetUser", _model.UserToEdit.UserName);
		Assert.AreEqual("target@example.com", _model.UserToEdit.Email);
		Assert.IsTrue(_model.UserToEdit.EmailConfirmed);
	}

	#endregion

	#region OnPost

	[TestMethod]
	public async Task OnPost_InvalidModelState_ReturnsPage()
	{
		var user = _db.AddUser("TestUser").Entity;
		AddAuthenticatedUser(_model, user, [PermissionTo.SeeEmails, PermissionTo.EditUsers]);
		_model.Id = user.Id;
		_model.ModelState.AddModelError("Email", "Invalid email format");

		var result = await _model.OnPost();

		Assert.IsInstanceOfType<PageResult>(result);
	}

	[TestMethod]
	public async Task OnPost_UserNotFound_ReturnsNotFound()
	{
		var user = _db.AddUser("TestUser").Entity;
		AddAuthenticatedUser(_model, user, [PermissionTo.SeeEmails, PermissionTo.EditUsers]);
		_model.Id = 999; // Non-existent user ID
		_model.UserToEdit = new EditEmailModel.UserEmailEditModel
		{
			Email = "new@example.com",
			EmailConfirmed = true
		};

		var result = await _model.OnPost();

		Assert.IsInstanceOfType<NotFoundResult>(result);
	}

	[TestMethod]
	public async Task OnPost_EmailAlreadyExists_AddsModelErrorAndReturnsPage()
	{
		var authenticatedUser = _db.AddUser("AuthenticatedUser").Entity;
		var targetUser = _db.AddUser("TargetUser").Entity;
		targetUser.Email = "target@example.com";
		var existingUser = _db.AddUser("ExistingUser").Entity;
		existingUser.Email = "existing@example.com";
		await _db.SaveChangesAsync();

		AddAuthenticatedUser(_model, authenticatedUser, [PermissionTo.SeeEmails, PermissionTo.EditUsers]);
		_model.Id = targetUser.Id;
		_model.UserToEdit = new EditEmailModel.UserEmailEditModel
		{
			UserName = "TargetUser",
			Email = "existing@example.com", // Same as existing user
			EmailConfirmed = false
		};

		_signInManager.EmailExists("existing@example.com").Returns(true);

		var result = await _model.OnPost();

		Assert.IsInstanceOfType<PageResult>(result);
		Assert.IsTrue(_model.ModelState.ContainsKey($"{nameof(_model.UserToEdit)}.{nameof(_model.UserToEdit.Email)}"));
		Assert.AreEqual("Email already exists.", _model.ModelState[$"{nameof(_model.UserToEdit)}.{nameof(_model.UserToEdit.Email)}"]!.Errors[0].ErrorMessage);
	}

	[TestMethod]
	public async Task OnPost_EmailCasingChanged_DoesNotTriggerEmailExistsError()
	{
		var authenticatedUser = _db.AddUser("AuthenticatedUser").Entity;
		var targetUser = _db.AddUser("TargetUser").Entity;
		targetUser.Email = "target@example.com";
		await _db.SaveChangesAsync();

		AddAuthenticatedUser(_model, authenticatedUser, [PermissionTo.SeeEmails, PermissionTo.EditUsers]);
		_model.Id = targetUser.Id;
		_model.UserToEdit = new EditEmailModel.UserEmailEditModel
		{
			UserName = "TargetUser",
			Email = "TARGET@EXAMPLE.COM", // Same email, different case
			EmailConfirmed = true
		};

		_userManager.NormalizeEmail("TARGET@EXAMPLE.COM").Returns("TARGET@EXAMPLE.COM");

		var result = await _model.OnPost();

		Assert.IsInstanceOfType<RedirectToPageResult>(result);
		await _signInManager.DidNotReceive().EmailExists(Arg.Any<string>());
	}

	[TestMethod]
	public async Task OnPost_ValidUpdate_UpdatesUserAndSendsNotifications()
	{
		var authenticatedUser = _db.AddUser("AuthenticatedUser").Entity;
		var targetUser = _db.AddUser("TargetUser").Entity;
		targetUser.Email = "old@example.com";
		targetUser.EmailConfirmed = false;
		await _db.SaveChangesAsync();

		AddAuthenticatedUser(_model, authenticatedUser, [PermissionTo.SeeEmails, PermissionTo.EditUsers]);
		_model.Id = targetUser.Id;
		_model.UserToEdit = new EditEmailModel.UserEmailEditModel
		{
			UserName = "TargetUser",
			Email = "new@example.com",
			EmailConfirmed = true
		};

		_signInManager.EmailExists("new@example.com").Returns(false);
		_userManager.NormalizeEmail("new@example.com").Returns("NEW@EXAMPLE.COM");

		var result = await _model.OnPost();

		Assert.IsInstanceOfType<RedirectToPageResult>(result);
		var redirect = (RedirectToPageResult)result;
		Assert.AreEqual("Edit", redirect.PageName);
		Assert.AreEqual(targetUser.Id, redirect.RouteValues!["Id"]);

		// Verify user was updated
		var updatedUser = await _db.Users.FindAsync(targetUser.Id);
		Assert.IsNotNull(updatedUser);
		Assert.AreEqual("new@example.com", updatedUser.Email);
		Assert.AreEqual("NEW@EXAMPLE.COM", updatedUser.NormalizedEmail);
		Assert.IsTrue(updatedUser.EmailConfirmed);

		// Verify notifications were sent
		await _publisher.Received(1).Send(Arg.Any<Post>());
		await _userMaintenanceLogger.Received(1).Log(
			targetUser.Id, "User TargetUser email changed by AuthenticatedUser", authenticatedUser.Id);
	}

	[TestMethod]
	public async Task OnPost_EmailConfirmedChanged_UpdatesConfirmationStatus()
	{
		var authenticatedUser = _db.AddUser("AuthenticatedUser").Entity;
		var targetUser = _db.AddUser("TargetUser").Entity;
		targetUser.Email = "user@example.com";
		targetUser.EmailConfirmed = true;
		await _db.SaveChangesAsync();

		AddAuthenticatedUser(_model, authenticatedUser, [PermissionTo.SeeEmails, PermissionTo.EditUsers]);
		_model.Id = targetUser.Id;
		_model.UserToEdit = new EditEmailModel.UserEmailEditModel
		{
			UserName = "TargetUser",
			Email = "user@example.com", // Same email
			EmailConfirmed = false // Changed confirmation status
		};

		_userManager.NormalizeEmail("user@example.com").Returns("USER@EXAMPLE.COM");

		var result = await _model.OnPost();

		Assert.IsInstanceOfType<RedirectToPageResult>(result);

		// Verify confirmation status was updated
		var updatedUser = await _db.Users.FindAsync(targetUser.Id);
		Assert.IsNotNull(updatedUser);
		Assert.IsFalse(updatedUser.EmailConfirmed);
	}

	#endregion
}
