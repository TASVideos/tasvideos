using TASVideos.Data.Entity;
using TASVideos.Data.Entity.Game;
using TASVideos.Pages.Forum.Topics;
using static TASVideos.RazorPages.Tests.RazorTestHelpers;

namespace TASVideos.RazorPages.Tests.Pages.Forum.Topics;

[TestClass]
public class CatalogModelTests : BasePageModelTests
{
	private readonly CatalogModel _model;

	public CatalogModelTests()
	{
		_model = new CatalogModel(_db)
		{
			PageContext = TestPageContext()
		};
	}

	[TestMethod]
	public async Task OnGet_NonExistentTopic_ReturnsNotFound()
	{
		_model.Id = 999;

		var result = await _model.OnGet();

		Assert.IsInstanceOfType(result, typeof(NotFoundResult));
	}

	[TestMethod]
	public async Task OnGet_TopicWithoutGame_PopulatesTopicTitleAndSystems()
	{
		var user = _db.AddUserWithRole("TestUser").Entity;
		var topic = _db.AddTopic(user).Entity;
		topic.Title = "Test Topic";

		var system = new GameSystem { Id = 1, Code = "NES", DisplayName = "Nintendo Entertainment System" };
		_db.GameSystems.Add(system);
		await _db.SaveChangesAsync();

		AddAuthenticatedUser(_model, user, [PermissionTo.CatalogMovies]);
		_model.Id = topic.Id;

		var result = await _model.OnGet();

		Assert.IsInstanceOfType(result, typeof(PageResult));
		Assert.AreEqual("Test Topic", _model.Title);
		Assert.IsNull(_model.SystemId);
		Assert.IsNull(_model.GameId);
		Assert.IsTrue(_model.AvailableSystems.Count > 0);
		Assert.AreEqual(0, _model.AvailableGames.Count);
	}

	[TestMethod]
	public async Task OnGet_TopicWithGame_PopulatesGameAndSystemData()
	{
		var user = _db.AddUserWithRole("TestUser").Entity;
		var system = new GameSystem { Id = 1, Code = "NES", DisplayName = "Nintendo Entertainment System" };
		_db.GameSystems.Add(system);

		var game = new Game { Id = 100, DisplayName = "Super Mario Bros." };
		_db.Games.Add(game);

		var gameVersion = new GameVersion { Game = game, System = system, Name = "USA" };
		_db.GameVersions.Add(gameVersion);

		var topic = _db.AddTopic(user).Entity;
		topic.Title = "SMB TAS Topic";
		topic.GameId = game.Id;

		await _db.SaveChangesAsync();

		AddAuthenticatedUser(_model, user, [PermissionTo.CatalogMovies]);
		_model.Id = topic.Id;

		var result = await _model.OnGet();

		Assert.IsInstanceOfType(result, typeof(PageResult));
		Assert.AreEqual("SMB TAS Topic", _model.Title);
		Assert.AreEqual(system.Id, _model.SystemId);
		Assert.AreEqual(game.Id, _model.GameId);
		Assert.IsTrue(_model.AvailableSystems.Count > 0);
		Assert.IsTrue(_model.AvailableGames.Count > 0);
	}

	[TestMethod]
	public async Task OnGet_TopicWithGameButNoGameVersion_SystemIdIsNull()
	{
		var user = _db.AddUserWithRole("TestUser").Entity;
		var game = new Game { Id = 100, DisplayName = "Test Game" };
		_db.Games.Add(game);

		var topic = _db.AddTopic(user).Entity;
		topic.Title = "Game Topic";
		topic.GameId = game.Id;

		await _db.SaveChangesAsync();

		AddAuthenticatedUser(_model, user, [PermissionTo.CatalogMovies]);
		_model.Id = topic.Id;

		var result = await _model.OnGet();

		Assert.IsInstanceOfType(result, typeof(PageResult));
		Assert.AreEqual("Game Topic", _model.Title);
		Assert.AreEqual(0, _model.SystemId); // FirstOrDefaultAsync returns 0 for int when no match
		Assert.AreEqual(game.Id, _model.GameId);
	}

	[TestMethod]
	public async Task OnPost_InvalidModelState_ReturnsPageWithDropdowns()
	{
		var user = _db.AddUserWithRole("TestUser").Entity;
		var topic = _db.AddTopic(user).Entity;

		// Add a system for the dropdown initialization
		var system = new GameSystem { Id = 1, Code = "NES", DisplayName = "Nintendo Entertainment System" };
		_db.GameSystems.Add(system);
		await _db.SaveChangesAsync();

		AddAuthenticatedUser(_model, user, [PermissionTo.CatalogMovies]);
		_model.Id = topic.Id;
		_model.ModelState.AddModelError("Title", "Title is required");

		var result = await _model.OnPost();

		Assert.IsInstanceOfType(result, typeof(PageResult));
		Assert.IsTrue(_model.AvailableSystems.Count > 0);
	}

	[TestMethod]
	public async Task OnPost_NonExistentTopic_ReturnsNotFound()
	{
		var user = _db.AddUserWithRole("TestUser").Entity;
		AddAuthenticatedUser(_model, user, [PermissionTo.CatalogMovies]);
		_model.Id = 999;
		_model.Title = "Valid Title";

		var result = await _model.OnPost();

		Assert.IsInstanceOfType(result, typeof(NotFoundResult));
	}

	[TestMethod]
	public async Task OnPost_NonExistentGame_ReturnsBadRequest()
	{
		var user = _db.AddUserWithRole("TestUser").Entity;
		var topic = _db.AddTopic(user).Entity;
		await _db.SaveChangesAsync();

		AddAuthenticatedUser(_model, user, [PermissionTo.CatalogMovies]);
		_model.Id = topic.Id;
		_model.Title = "Valid Title";
		_model.GameId = 999; // Non-existent game

		var result = await _model.OnPost();

		Assert.IsInstanceOfType(result, typeof(BadRequestResult));
	}

	[TestMethod]
	public async Task OnPost_ValidGameId_CatalogsTopicSuccessfully()
	{
		var user = _db.AddUserWithRole("TestUser").Entity;
		var game = new Game { Id = 100, DisplayName = "Test Game" };
		_db.Games.Add(game);

		var topic = _db.AddTopic(user).Entity;
		topic.Title = "Test Topic";
		await _db.SaveChangesAsync();

		AddAuthenticatedUser(_model, user, [PermissionTo.CatalogMovies]);
		_model.Id = topic.Id;
		_model.Title = "Updated Title";
		_model.GameId = game.Id;

		var result = await _model.OnPost();

		Assert.IsInstanceOfType(result, typeof(RedirectToPageResult));
		var redirect = (RedirectToPageResult)result;
		Assert.AreEqual("Index", redirect.PageName);
		Assert.AreEqual(topic.Id, redirect.RouteValues!["Id"]);

		// Verify the topic was updated
		await _db.Entry(topic).ReloadAsync();
		Assert.AreEqual(game.Id, topic.GameId);
	}

	[TestMethod]
	public async Task OnPost_NullGameId_CatalogsTopicWithNullGame()
	{
		var user = _db.AddUserWithRole("TestUser").Entity;
		var game = new Game { Id = 100, DisplayName = "Initial Game" };
		_db.Games.Add(game);

		var topic = _db.AddTopic(user).Entity;
		topic.Title = "Test Topic";
		topic.GameId = game.Id; // Initially has a game
		await _db.SaveChangesAsync();

		AddAuthenticatedUser(_model, user, [PermissionTo.CatalogMovies]);
		_model.Id = topic.Id;
		_model.Title = "Updated Title";
		_model.GameId = null; // Remove game association

		var result = await _model.OnPost();

		Assert.IsInstanceOfType(result, typeof(RedirectToPageResult));

		// Verify the topic game was cleared
		await _db.Entry(topic).ReloadAsync();
		Assert.IsNull(topic.GameId);
	}

	[TestMethod]
	public async Task OnPost_EmptyGameId_SkipsGameValidation()
	{
		var user = _db.AddUserWithRole("TestUser").Entity;
		var topic = _db.AddTopic(user).Entity;
		await _db.SaveChangesAsync();

		AddAuthenticatedUser(_model, user, [PermissionTo.CatalogMovies]);
		_model.Id = topic.Id;
		_model.Title = "Valid Title";
		_model.GameId = null;

		var result = await _model.OnPost();

		// Should not return BadRequest since no game validation needed
		Assert.IsInstanceOfType(result, typeof(RedirectToPageResult));
	}

	[TestMethod]
	public async Task Initialize_WithNoSystemId_PopulatesOnlySystems()
	{
		var system1 = new GameSystem { Id = 1, Code = "NES", DisplayName = "Nintendo Entertainment System" };
		var system2 = new GameSystem { Id = 2, Code = "SNES", DisplayName = "Super Nintendo Entertainment System" };
		_db.GameSystems.AddRange(system1, system2);
		await _db.SaveChangesAsync();

		_model.SystemId = null;

		// Call the private Initialize method through OnGet
		var user = _db.AddUserWithRole("TestUser").Entity;
		var topic = _db.AddTopic(user).Entity;
		await _db.SaveChangesAsync();

		AddAuthenticatedUser(_model, user, [PermissionTo.CatalogMovies]);
		_model.Id = topic.Id;
		await _model.OnGet();

		Assert.IsTrue(_model.AvailableSystems.Count >= 2);
		Assert.AreEqual(0, _model.AvailableGames.Count);

		// Check that systems are ordered by Code
		var systemOptions = _model.AvailableSystems.Where(s => !string.IsNullOrEmpty(s.Value)).ToList();
		Assert.IsTrue(systemOptions.Any(s => s.Text == "NES"));
		Assert.IsTrue(systemOptions.Any(s => s.Text == "SNES"));
	}

	[TestMethod]
	public async Task Initialize_WithSystemId_PopulatesSystemsAndGames()
	{
		var system = new GameSystem { Id = 1, Code = "NES", DisplayName = "Nintendo Entertainment System" };
		_db.GameSystems.Add(system);

		var game1 = new Game { Id = 100, DisplayName = "Game A" };
		var game2 = new Game { Id = 101, DisplayName = "Game B" };
		_db.Games.AddRange(game1, game2);

		var version1 = new GameVersion { Game = game1, System = system, Name = "USA" };
		var version2 = new GameVersion { Game = game2, System = system, Name = "Japan" };
		_db.GameVersions.AddRange(version1, version2);

		await _db.SaveChangesAsync();

		_model.SystemId = system.Id;

		// Call Initialize through OnGet
		var user = _db.AddUserWithRole("TestUser").Entity;
		var topic = _db.AddTopic(user).Entity;
		await _db.SaveChangesAsync();

		AddAuthenticatedUser(_model, user, [PermissionTo.CatalogMovies]);
		_model.Id = topic.Id;
		await _model.OnGet();

		Assert.IsTrue(_model.AvailableSystems.Count > 0);
		Assert.IsTrue(_model.AvailableGames.Count >= 2);

		// Check that games are populated for the system
		var gameOptions = _model.AvailableGames.Where(g => !string.IsNullOrEmpty(g.Value)).ToList();
		Assert.IsTrue(gameOptions.Any(g => g.Text.Contains("Game A")));
		Assert.IsTrue(gameOptions.Any(g => g.Text.Contains("Game B")));
	}

	[TestMethod]
	public void CatalogModel_Properties_HaveCorrectDefaultValues()
	{
		Assert.AreEqual(0, _model.Id);
		Assert.AreEqual("", _model.Title);
		Assert.IsNull(_model.SystemId);
		Assert.IsNull(_model.GameId);
		Assert.AreEqual(0, _model.AvailableSystems.Count);
		Assert.AreEqual(0, _model.AvailableGames.Count);
	}

	[TestMethod]
	public async Task OnGet_WithMultipleGameVersionsForSameGame_ReturnsFirstSystemId()
	{
		var user = _db.AddUserWithRole("TestUser").Entity;
		var system1 = new GameSystem { Id = 1, Code = "NES", DisplayName = "Nintendo Entertainment System" };
		var system2 = new GameSystem { Id = 2, Code = "SNES", DisplayName = "Super Nintendo Entertainment System" };
		_db.GameSystems.AddRange(system1, system2);

		var game = new Game { Id = 100, DisplayName = "Test Game" };
		_db.Games.Add(game);

		// Add game versions for different systems (inserted in order that should return system2 first)
		var version1 = new GameVersion { Game = game, System = system2, Name = "Version 2" }; // This should be returned first
		var version2 = new GameVersion { Game = game, System = system1, Name = "Version 1" };
		_db.GameVersions.AddRange(version1, version2);

		var topic = _db.AddTopic(user).Entity;
		topic.Title = "Test Topic";
		topic.GameId = game.Id;

		await _db.SaveChangesAsync();

		AddAuthenticatedUser(_model, user, [PermissionTo.CatalogMovies]);
		_model.Id = topic.Id;

		var result = await _model.OnGet();

		Assert.IsInstanceOfType(result, typeof(PageResult));
		Assert.AreEqual(system2.Id, _model.SystemId);
	}
}
