using TASVideos.Data.Entity.Game;
using TASVideos.Pages.UserFiles;
using TASVideos.Tests.Base;

namespace TASVideos.RazorPages.Tests.Pages.UserFiles;

[TestClass]
public class CatalogModelTests : TestDbBase
{
	private readonly CatalogModel _page;

	public CatalogModelTests()
	{
		_page = new CatalogModel(_db);
	}

	[TestMethod]
	public async Task OnGet_WithNonExistentId_ReturnsNotFound()
	{
		_page.Id = 999;
		var result = await _page.OnGet();
		Assert.IsInstanceOfType<NotFoundResult>(result);
	}

	[TestMethod]
	public async Task OnGet_WithValidId_LoadsUserFile()
	{
		var system = _db.AddGameSystem("NES").Entity;
		var game = _db.AddGame("SMB").Entity;
		var author = _db.AddUser("TestAuthor").Entity;
		var userFile = _db.AddPublicUserFile(author).Entity;
		userFile.System = system;
		userFile.Game = game;
		await _db.SaveChangesAsync();
		_page.Id = userFile.Id;

		var result = await _page.OnGet();

		Assert.IsInstanceOfType<PageResult>(result);
		Assert.AreEqual(userFile.Id, _page.UserFile.Id);
		Assert.AreEqual(system.Id, _page.UserFile.System);
		Assert.AreEqual(game.Id, _page.UserFile.Game);
		Assert.AreEqual("test.bk2", _page.UserFile.Filename);
		Assert.AreEqual("TestAuthor", _page.UserFile.AuthorName);
		Assert.IsTrue(_page.AvailableSystems.Count > 0);
		Assert.IsTrue(_page.AvailableGames.Count > 0);
	}

	[TestMethod]
	public async Task OnGet_WithValidIdButNoGameOrSystem_Loads()
	{
		var author = _db.AddUser("TestAuthor").Entity;
		var userFile = _db.AddPublicUserFile(author, "uncategorized.bk2").Entity;
		await _db.SaveChangesAsync();

		_page.Id = userFile.Id;

		var result = await _page.OnGet();

		Assert.IsInstanceOfType<PageResult>(result);
		Assert.AreEqual(userFile.Id, _page.UserFile.Id);
		Assert.IsNull(_page.UserFile.System);
		Assert.IsNull(_page.UserFile.Game);
		Assert.AreEqual("uncategorized.bk2", _page.UserFile.Filename);
		Assert.AreEqual("TestAuthor", _page.UserFile.AuthorName);
		Assert.IsTrue(_page.AvailableSystems.Count > 0);
		Assert.AreEqual(0, _page.AvailableGames.Count); // No games because no system selected
	}

	[TestMethod]
	public async Task OnPost_InvalidModelState_ReturnsPage()
	{
		_db.AddGameSystem("NES");
		var author = _db.AddUser("TestAuthor").Entity;
		var userFile = _db.AddPublicUserFile(author).Entity;
		await _db.SaveChangesAsync();

		var user = _db.AddUser("TestUser").Entity;
		AddAuthenticatedUser(_page, user, [PermissionTo.CatalogMovies]);

		_page.Id = userFile.Id;
		_page.ModelState.AddModelError("System", "System is required");

		var result = await _page.OnPost();

		Assert.IsInstanceOfType<PageResult>(result);
		Assert.IsFalse(_page.ModelState.IsValid);
		Assert.IsTrue(_page.AvailableSystems.Count > 0);
	}

	[TestMethod]
	public async Task OnPost_UserFileNotFound_UpdatesNothing()
	{
		_page.Id = 999; // Non-existent ID
		_page.UserFile = new CatalogModel.Catalog
		{
			Id = 999,
			System = 1,
			Game = 1,
			Filename = "test.bk2",
			AuthorName = "TestAuthor"
		};

		var result = await _page.OnPost();

		Assert.IsInstanceOfType<RedirectToPageResult>(result);
		var redirect = (RedirectToPageResult)result;
		Assert.AreEqual("Info", redirect.PageName);
		Assert.AreEqual(999L, redirect.RouteValues!["Id"]);
	}

	[TestMethod]
	public async Task OnPost_ValidUpdate_UpdatesUserFileAndRedirects()
	{
		var system1 = _db.AddGameSystem("NES").Entity;
		var system2 = _db.Add(new GameSystem { Id = 2, Code = "SNES" }).Entity;

		var game1 = _db.AddGame("Original Game").Entity;
		var game2 = _db.AddGame("New Game").Entity;

		var author = _db.AddUser("TestAuthor").Entity;
		var userFile = _db.AddPublicUserFile(author).Entity;
		userFile.System = system1;
		userFile.Game = game1;
		await _db.SaveChangesAsync();

		var user = _db.AddUser("TestUser").Entity;
		AddAuthenticatedUser(_page, user, [PermissionTo.CatalogMovies]);
		await _db.SaveChangesAsync();

		_page.Id = userFile.Id;
		_page.UserFile = new CatalogModel.Catalog
		{
			Id = userFile.Id,
			System = system2.Id,
			Game = game2.Id,
			Filename = "test.bk2",
			AuthorName = "TestAuthor"
		};

		var result = await _page.OnPost();

		Assert.IsInstanceOfType<RedirectToPageResult>(result);
		var redirect = (RedirectToPageResult)result;
		Assert.AreEqual("Info", redirect.PageName);
		Assert.AreEqual(userFile.Id, redirect.RouteValues!["Id"]);

		_db.ChangeTracker.Clear();
		var updatedUserFile = await _db.UserFiles.FindAsync(userFile.Id);
		Assert.IsNotNull(updatedUserFile);
		Assert.AreEqual(system2.Id, updatedUserFile.SystemId);
		Assert.AreEqual(game2.Id, updatedUserFile.GameId);
	}

	[TestMethod]
	public async Task OnPost_UpdateToNullValues_ClearsSystemAndGame()
	{
		var system = _db.AddGameSystem("NES").Entity;
		var game = _db.AddGame("Test Game").Entity;
		var author = _db.AddUser("TestAuthor").Entity;
		var userFile = _db.AddPublicUserFile(author).Entity;
		userFile.System = system;
		userFile.Game = game;
		await _db.SaveChangesAsync();

		var user = _db.AddUser("TestUser").Entity;
		AddAuthenticatedUser(_page, user, [PermissionTo.CatalogMovies]);

		_page.Id = userFile.Id;
		_page.UserFile = new CatalogModel.Catalog
		{
			Id = userFile.Id,
			System = null, // Clear system
			Game = null,   // Clear game
			Filename = "test.bk2",
			AuthorName = "TestAuthor"
		};

		var result = await _page.OnPost();

		Assert.IsInstanceOfType<RedirectToPageResult>(result);

		_db.ChangeTracker.Clear();
		var updatedUserFile = await _db.UserFiles.FindAsync(userFile.Id);
		Assert.IsNotNull(updatedUserFile);
		Assert.IsNull(updatedUserFile.SystemId);
		Assert.IsNull(updatedUserFile.GameId);
	}

	[TestMethod]
	public async Task OnPost_NoActualChanges_StillRedirects()
	{
		var system = _db.AddGameSystem("NES").Entity;
		var game = _db.AddGame("Test Game").Entity;
		var author = _db.AddUser("TestAuthor").Entity;
		var userFile = _db.AddPublicUserFile(author).Entity;
		userFile.System = system;
		userFile.Game = game;
		await _db.SaveChangesAsync();

		var user = _db.AddUser("TestUser").Entity;
		AddAuthenticatedUser(_page, user, [PermissionTo.CatalogMovies]);

		_page.Id = userFile.Id;
		_page.UserFile = new CatalogModel.Catalog
		{
			Id = userFile.Id,
			System = system.Id, // Same system
			Game = game.Id,     // Same game
			Filename = "test.bk2",
			AuthorName = "TestAuthor"
		};

		var result = await _page.OnPost();

		Assert.IsInstanceOfType<RedirectToPageResult>(result);

		_db.ChangeTracker.Clear();
		var updatedUserFile = await _db.UserFiles.FindAsync(userFile.Id);
		Assert.IsNotNull(updatedUserFile);
		Assert.AreEqual(system.Id, updatedUserFile.SystemId);
		Assert.AreEqual(game.Id, updatedUserFile.GameId);
	}

	[TestMethod]
	public async Task Initialize_WithNoSystem_PopulatesOnlyAvailableSystems()
	{
		_db.AddGameSystem("NES");
		var author = _db.AddUser("TestAuthor").Entity;
		var userFile = _db.AddPublicUserFile(author).Entity;
		await _db.SaveChangesAsync();

		_page.Id = userFile.Id;
		_page.UserFile = new CatalogModel.Catalog
		{
			Id = userFile.Id,
			System = null,
			Game = null,
			Filename = "test.bk2",
			AuthorName = "TestAuthor"
		};

		await _page.OnGet();

		Assert.IsTrue(_page.AvailableSystems.Count > 0);
		Assert.AreEqual(0, _page.AvailableGames.Count);
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

		_page.Id = userFile.Id;

		await _page.OnGet();

		Assert.IsTrue(_page.AvailableSystems.Count > 0);
		Assert.IsTrue(_page.AvailableGames.Count > 0);
		Assert.AreEqual(system.Id, _page.UserFile.System);
		Assert.AreEqual(game.Id, _page.UserFile.Game);
	}

	[TestMethod]
	public void RequiresPermission() => AssertHasPermission(typeof(CatalogModel), PermissionTo.CatalogMovies);
}
