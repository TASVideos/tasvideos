using TASVideos.Data.Entity.Game;
using TASVideos.Pages.Forum.Topics;

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

		_db.AddGameSystem("NES");
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
		var system = _db.AddGameSystem("NES").Entity;
		var game = _db.AddGame("Super Mario Bros.").Entity;
		_db.GameVersions.Add(new GameVersion { Game = game, System = system, Name = "USA" });
		var topic = _db.AddTopic().Entity;
		topic.Title = "SMB TAS Topic";
		topic.Game = game;
		await _db.SaveChangesAsync();
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
		var game = _db.AddGame("Test Game").Entity;
		var topic = _db.AddTopic().Entity;
		topic.Game = game;
		await _db.SaveChangesAsync();
		_model.Id = topic.Id;

		var result = await _model.OnGet();

		Assert.IsInstanceOfType(result, typeof(PageResult));
		Assert.AreEqual(0, _model.SystemId);
		Assert.AreEqual(game.Id, _model.GameId);
	}

	[TestMethod]
	public async Task OnPost_InvalidModelState_ReturnsPageWithDropdowns()
	{
		_db.AddGameSystem("NES");
		await _db.SaveChangesAsync();
		_model.Id = 1;
		_model.ModelState.AddModelError("Title", "Title is required");

		var result = await _model.OnPost();

		Assert.IsInstanceOfType(result, typeof(PageResult));
		Assert.IsTrue(_model.AvailableSystems.Count > 0);
	}

	[TestMethod]
	public async Task OnPost_NonExistentTopic_ReturnsNotFound()
	{
		_model.Id = 999;
		var result = await _model.OnPost();
		Assert.IsInstanceOfType(result, typeof(NotFoundResult));
	}

	[TestMethod]
	public async Task OnPost_NonExistentGame_ReturnsBadRequest()
	{
		var topic = _db.AddTopic().Entity;
		await _db.SaveChangesAsync();
		_model.Id = topic.Id;
		_model.GameId = 999; // Non-existent game

		var result = await _model.OnPost();

		AssertBadRequest(result);
	}

	[TestMethod]
	public async Task OnPost_ValidGameId_CatalogsTopicSuccessfully()
	{
		var game = _db.AddGame("Test Game").Entity;
		var topic = _db.AddTopic().Entity;
		await _db.SaveChangesAsync();
		_model.Id = topic.Id;
		_model.GameId = game.Id;

		var result = await _model.OnPost();

		AssertRedirect(result, "Index", topic.Id);
		await _db.Entry(topic).ReloadAsync();
		Assert.AreEqual(game.Id, topic.GameId);
	}

	[TestMethod]
	public async Task OnPost_NullGameId_CatalogsTopicWithNullGame()
	{
		var topic = _db.AddTopic().Entity;
		topic.Game = _db.AddGame("Initial Game").Entity;
		await _db.SaveChangesAsync();

		_model.Id = topic.Id;
		_model.GameId = null; // Remove game association

		var result = await _model.OnPost();

		AssertRedirect(result, "Index");
		await _db.Entry(topic).ReloadAsync();
		Assert.IsNull(topic.GameId);
	}

	[TestMethod]
	public async Task OnPost_EmptyGameId_SkipsGameValidation()
	{
		var topic = _db.AddTopic().Entity;
		await _db.SaveChangesAsync();
		_model.Id = topic.Id;
		_model.Title = "Valid Title";
		_model.GameId = null;

		var result = await _model.OnPost();

		// Should not return BadRequest since no game validation needed
		AssertRedirect(result, "Index");
	}

	[TestMethod]
	public async Task Initialize_WithNoSystemId_PopulatesOnlySystems()
	{
		_db.AddGameSystem("NES");
		var user = _db.AddUserWithRole("TestUser").Entity;
		var topic = _db.AddTopic(user).Entity;
		await _db.SaveChangesAsync();
		_model.Id = topic.Id;
		_model.SystemId = null;

		await _model.OnGet();

		Assert.AreEqual(1, _model.AvailableSystems.Count(s => s.Text == "NES"));
		Assert.AreEqual(0, _model.AvailableGames.Count);
	}
}
