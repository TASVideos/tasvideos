using TASVideos.Data.Entity.Game;
using TASVideos.Pages.UserFiles;
using TASVideos.Tests.Base;

namespace TASVideos.RazorPages.Tests.Pages.UserFiles;

[TestClass]
public class EditModelTests : TestDbBase
{
	private readonly EditModel _page;

	public EditModelTests()
	{
		_page = new EditModel(_db);
	}

	[TestMethod]
	public async Task OnGet_WithNonExistentId_ReturnsNotFound()
	{
		_page.Id = 999;
		var result = await _page.OnGet();
		Assert.IsInstanceOfType<NotFoundResult>(result);
	}

	[TestMethod]
	public async Task OnGet_AsAuthor_LoadsUserFileData()
	{
		var system = _db.AddGameSystem("NES").Entity;
		var game = _db.AddGame("Test Game").Entity;
		var author = _db.AddUser("TestAuthor").Entity;
		var userFile = _db.AddPublicUserFile(author, title: "Title", description: "Description").Entity;
		userFile.System = system;
		userFile.Game = game;
		await _db.SaveChangesAsync();

		AddAuthenticatedUser(_page, author, []);
		_page.Id = userFile.Id;

		var result = await _page.OnGet();

		Assert.IsInstanceOfType<PageResult>(result);
		Assert.AreEqual("Title", _page.UserFile.Title);
		Assert.AreEqual("Description", _page.UserFile.Description);
		Assert.AreEqual(system.Id, _page.UserFile.System);
		Assert.AreEqual(game.Id, _page.UserFile.Game);
		Assert.IsFalse(_page.UserFile.Hidden);
		Assert.AreEqual(author.Id, _page.UserFile.UserId);
		Assert.AreEqual("TestAuthor", _page.UserFile.UserName);
		Assert.IsTrue(_page.AvailableSystems.Count > 0);
		Assert.IsTrue(_page.AvailableGames.Count > 0);
	}

	[TestMethod]
	public async Task OnGet_WithEditPermission_LoadsUserFileData()
	{
		var author = _db.AddUser("FileAuthor").Entity;
		var editor = _db.AddUser("FileEditor").Entity;
		var userFile = _db.AddHiddenUserFile(author, "test.bk2", "Title", "Description").Entity;
		await _db.SaveChangesAsync();

		AddAuthenticatedUser(_page, editor, [PermissionTo.EditUserFiles]);
		_page.Id = userFile.Id;

		var result = await _page.OnGet();

		Assert.IsInstanceOfType<PageResult>(result);
		Assert.AreEqual("Title", _page.UserFile.Title);
		Assert.AreEqual("Description", _page.UserFile.Description);
		Assert.IsTrue(_page.UserFile.Hidden);
		Assert.AreEqual(author.Id, _page.UserFile.UserId);
		Assert.AreEqual("FileAuthor", _page.UserFile.UserName);
	}

	[TestMethod]
	public async Task OnGet_WithoutPermissionOrAuthor_ReturnsAccessDenied()
	{
		var author = _db.AddUser("FileAuthor").Entity;
		var otherUser = _db.AddUser("OtherUser").Entity;
		var userFile = _db.AddPublicUserFile(author, "test.bk2").Entity;
		await _db.SaveChangesAsync();

		AddAuthenticatedUser(_page, otherUser, []);
		_page.Id = userFile.Id;

		var result = await _page.OnGet();

		AssertAccessDenied(result);
	}

	[TestMethod]
	public async Task OnGet_WithSystemButNoGame_LoadsSystemDropdownOnly()
	{
		var system = _db.AddGameSystem("NES").Entity;
		var author = _db.AddUser("TestAuthor").Entity;
		var userFile = _db.AddPublicUserFile(author, "test.bk2").Entity;
		userFile.System = system;
		await _db.SaveChangesAsync();

		AddAuthenticatedUser(_page, author, []);
		_page.Id = userFile.Id;

		var result = await _page.OnGet();

		Assert.IsInstanceOfType<PageResult>(result);
		Assert.AreEqual(system.Id, _page.UserFile.System);
		Assert.IsNull(_page.UserFile.Game);
		Assert.IsTrue(_page.AvailableSystems.Count > 0);
		Assert.IsTrue(_page.AvailableGames.Count > 0); // Should have games for the selected system
	}

	[TestMethod]
	public async Task OnGet_WithNoSystemOrGame_LoadsSystemsOnlyDropdown()
	{
		var author = _db.AddUser("TestAuthor").Entity;
		var userFile = _db.AddPublicUserFile(author, "test.bk2").Entity;
		await _db.SaveChangesAsync();

		AddAuthenticatedUser(_page, author, []);
		_page.Id = userFile.Id;

		var result = await _page.OnGet();

		Assert.IsInstanceOfType<PageResult>(result);
		Assert.IsNull(_page.UserFile.System);
		Assert.IsNull(_page.UserFile.Game);
		Assert.IsTrue(_page.AvailableSystems.Count > 0);
		Assert.AreEqual(1, _page.AvailableGames.Count); // Should only have default entry
	}

	[TestMethod]
	public async Task OnPost_InvalidModelState_ReturnsPage()
	{
		var author = _db.AddUser("TestAuthor").Entity;
		var userFile = _db.AddPublicUserFile(author).Entity;
		await _db.SaveChangesAsync();

		AddAuthenticatedUser(_page, author, []);
		_page.Id = userFile.Id;
		_page.UserFile = new EditModel.UserFileEdit
		{
			UserId = author.Id,
			UserName = "TestAuthor"
		};
		_page.ModelState.AddModelError("Title", "Title is too long");

		var result = await _page.OnPost();

		Assert.IsInstanceOfType<PageResult>(result);
		Assert.IsFalse(_page.ModelState.IsValid);
		Assert.IsTrue(_page.AvailableSystems.Count > 0);
	}

	[TestMethod]
	public async Task OnPost_UserFileNotFound_ReturnsNotFound()
	{
		_page.Id = 999; // Non-existent ID
		var result = await _page.OnPost();
		Assert.IsInstanceOfType<NotFoundResult>(result);
	}

	[TestMethod]
	public async Task OnPost_WithoutPermissionOrAuthor_ReturnsAccessDenied()
	{
		var author = _db.AddUser("FileAuthor").Entity;
		var otherUser = _db.AddUser("OtherUser").Entity;
		var userFile = _db.AddPublicUserFile(author).Entity;
		await _db.SaveChangesAsync();

		// Set up user without permission and not as author
		AddAuthenticatedUser(_page, otherUser, []);
		_page.Id = userFile.Id;
		_page.UserFile = new EditModel.UserFileEdit
		{
			Title = "Updated Title",
			UserId = otherUser.Id,
			UserName = "OtherUser"
		};

		var result = await _page.OnPost();

		AssertAccessDenied(result);
	}

	[TestMethod]
	public async Task OnPost_AsAuthor_UpdatesUserFileAndRedirects()
	{
		var system1 = _db.AddGameSystem("NES").Entity;
		var system2 = _db.GameSystems.Add(new GameSystem { Id = 2, Code = "SNES" }).Entity;
		var game1 = _db.AddGame("Original Game").Entity;
		var game2 = new Game { Id = 2, DisplayName = "New Game" };
		_db.Games.AddRange(game1, game2);
		var author = _db.AddUser("TestAuthor").Entity;
		var userFile = _db.AddPublicUserFile(author, "test.bk2", "Original Title", "Original description").Entity;
		userFile.System = system1;
		userFile.Game = game1;
		await _db.SaveChangesAsync();

		AddAuthenticatedUser(_page, author, []);
		_page.Id = userFile.Id;
		_page.UserFile = new EditModel.UserFileEdit
		{
			Title = "Updated Title",
			Description = "Updated description",
			System = system2.Id,
			Game = game2.Id,
			Hidden = true,
			UserId = author.Id,
			UserName = "TestAuthor"
		};

		var result = await _page.OnPost();

		Assert.IsInstanceOfType<RedirectToPageResult>(result);
		var redirect = (RedirectToPageResult)result;
		Assert.AreEqual("/UserFiles/Info", redirect.PageName);
		Assert.AreEqual(userFile.Id, redirect.RouteValues!["Id"]);

		var updatedFile = await _db.UserFiles.FindAsync(userFile.Id);
		Assert.IsNotNull(updatedFile);
		Assert.AreEqual("Updated Title", updatedFile.Title);
		Assert.AreEqual("Updated description", updatedFile.Description);
		Assert.AreEqual(system2.Id, updatedFile.SystemId);
		Assert.AreEqual(game2.Id, updatedFile.GameId);
		Assert.IsTrue(updatedFile.Hidden);
	}

	[TestMethod]
	public async Task OnPost_WithEditPermission_UpdatesUserFileAndRedirects()
	{
		var author = _db.AddUser("FileAuthor").Entity;
		var editor = _db.AddUser("FileEditor").Entity;
		var userFile = _db.AddPublicUserFile(author).Entity;
		await _db.SaveChangesAsync();

		AddAuthenticatedUser(_page, editor, [PermissionTo.EditUserFiles]);
		_page.Id = userFile.Id;
		_page.UserFile = new EditModel.UserFileEdit
		{
			Title = "Editor Updated Title",
			Description = "Editor updated description",
			Hidden = true,
			UserName = "FileEditor"
		};

		var result = await _page.OnPost();

		Assert.IsInstanceOfType<RedirectToPageResult>(result);

		var updatedFile = await _db.UserFiles.FindAsync(userFile.Id);
		Assert.IsNotNull(updatedFile);
		Assert.AreEqual("Editor Updated Title", updatedFile.Title);
		Assert.AreEqual("Editor updated description", updatedFile.Description);
		Assert.IsTrue(updatedFile.Hidden);
	}

	[TestMethod]
	public async Task OnPost_UpdateToNullSystemAndGame_ClearsAssociations()
	{
		var author = _db.AddUser("TestAuthor").Entity;
		var userFile = _db.AddPublicUserFile(author).Entity;
		userFile.System = _db.AddGameSystem("NES").Entity;
		userFile.Game = _db.AddGame("Test Game").Entity;
		await _db.SaveChangesAsync();

		AddAuthenticatedUser(_page, author, []);
		_page.Id = userFile.Id;
		_page.UserFile = new EditModel.UserFileEdit
		{
			Title = "Test Title",
			System = null, // Clear system
			Game = null,   // Clear game
			UserId = author.Id,
			UserName = "TestAuthor"
		};

		var result = await _page.OnPost();

		Assert.IsInstanceOfType<RedirectToPageResult>(result);

		var updatedFile = await _db.UserFiles.FindAsync(userFile.Id);
		Assert.IsNotNull(updatedFile);
		Assert.IsNull(updatedFile.SystemId);
		Assert.IsNull(updatedFile.GameId);
	}

	[TestMethod]
	public async Task OnPost_NoActualChanges_StillRedirects()
	{
		var author = _db.AddUser("TestAuthor").Entity;
		await _db.SaveChangesAsync();
		var userFile = _db.AddPublicUserFile(author, "test.bk2", "Same Title", "Same description").Entity;
		await _db.SaveChangesAsync();

		AddAuthenticatedUser(_page, author, []);
		_page.Id = userFile.Id;
		_page.UserFile = new EditModel.UserFileEdit
		{
			Title = "Same Title",
			Description = "Same description",
			Hidden = false,
			UserId = author.Id,
			UserName = "TestAuthor"
		};

		var result = await _page.OnPost();

		Assert.IsInstanceOfType<RedirectToPageResult>(result);

		var updatedFile = await _db.UserFiles.FindAsync(userFile.Id);
		Assert.IsNotNull(updatedFile);
		Assert.AreEqual("Same Title", updatedFile.Title);
		Assert.AreEqual("Same description", updatedFile.Description);
		Assert.IsFalse(updatedFile.Hidden);
	}

	[TestMethod]
	public async Task Initialize_WithNoSystem_PopulatesOnlyAvailableSystems()
	{
		_db.AddGameSystem("NES");
		var author = _db.AddUser("TestAuthor").Entity;
		var userFile = _db.AddPublicUserFile(author).Entity;
		await _db.SaveChangesAsync();

		AddAuthenticatedUser(_page, author, []);
		_page.Id = userFile.Id;

		await _page.OnGet();

		Assert.IsTrue(_page.AvailableSystems.Count > 0);
		Assert.AreEqual(1, _page.AvailableGames.Count); // Only default entry
	}

	[TestMethod]
	public async Task Initialize_WithSelectedSystem_PopulatesSystemsAndGames()
	{
		var system = _db.AddGameSystem("NES").Entity;
		var game = _db.AddGame("Test Game").Entity;
		var author = _db.AddUser("TestAuthor").Entity;
		var userFile = _db.AddPublicUserFile(author).Entity;
		userFile.System = system;
		userFile.Game = game;
		await _db.SaveChangesAsync();

		AddAuthenticatedUser(_page, author, []);
		_page.Id = userFile.Id;

		await _page.OnGet();

		Assert.IsTrue(_page.AvailableSystems.Count > 0);
		Assert.IsTrue(_page.AvailableGames.Count > 0);
		Assert.AreEqual(system.Id, _page.UserFile.System);
		Assert.AreEqual(game.Id, _page.UserFile.Game);
	}

	[TestMethod]
	public void AllowsAnonymousAttribute() => AssertAllowsAnonymousUsers(typeof(EditModel));
}
