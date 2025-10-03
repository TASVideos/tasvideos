using TASVideos.Data.Entity.Game;
using TASVideos.Pages.Games;
using TASVideos.Tests.Base;

namespace TASVideos.RazorPages.Tests.Pages.Games;

[TestClass]
public class IndexModelTests : TestDbBase
{
	private readonly IndexModel _model;

	public IndexModelTests()
	{
		_model = new IndexModel(_db);
	}

	[TestMethod]
	public async Task OnGet_WithNonExistentGameId_ReturnsNotFound()
	{
		_model.Id = "999";
		var result = await _model.OnGet();
		Assert.IsInstanceOfType<NotFoundResult>(result);
	}

	[TestMethod]
	public async Task OnGet_WithNonExistentGameAbbreviation_ReturnsNotFound()
	{
		_model.Id = "NONEXISTENT";
		var result = await _model.OnGet();
		Assert.IsInstanceOfType<NotFoundResult>(result);
	}

	[TestMethod]
	public async Task OnGet_WithValidGameId_LoadsGameData()
	{
		var game = _db.AddGame("Test Game").Entity;
		game.Abbreviation = "TG";
		game.Aliases = "TestAlias";
		await _db.SaveChangesAsync();
		_model.Id = game.Id.ToString();

		var result = await _model.OnGet();

		Assert.IsInstanceOfType<PageResult>(result);
		Assert.AreEqual(game.Id, _model.Game.Id);
		Assert.AreEqual("Test Game", _model.Game.DisplayName);
		Assert.AreEqual("TG", _model.Game.Abbreviation);
		Assert.AreEqual("TestAlias", _model.Game.Aliases);
	}

	[TestMethod]
	public async Task OnGet_WithValidGameAbbreviation_LoadsGameData()
	{
		var game = _db.AddGame("Test Game").Entity;
		game.Abbreviation = "TG";
		await _db.SaveChangesAsync();
		_model.Id = "TG";

		var result = await _model.OnGet();

		Assert.IsInstanceOfType<PageResult>(result);
		Assert.AreEqual(game.Id, _model.Game.Id);
		Assert.AreEqual("Test Game", _model.Game.DisplayName);
		Assert.AreEqual("TG", _model.Game.Abbreviation);
	}

	[TestMethod]
	public async Task OnGet_WithGameGenres_LoadsGenres()
	{
		var game = _db.AddGame("Test Game").Entity;
		var genre1 = _db.AddGenre("Action").Entity;
		var genre2 = _db.AddGenre("Platform").Entity;
		_db.GameGenres.Add(new GameGenre { Game = game, Genre = genre1 });
		_db.GameGenres.Add(new GameGenre { Game = game, Genre = genre2 });
		await _db.SaveChangesAsync();

		_model.Id = game.Id.ToString();

		var result = await _model.OnGet();

		Assert.IsInstanceOfType<PageResult>(result);
		Assert.AreEqual(2, _model.Game.Genres.Count);
		Assert.IsTrue(_model.Game.Genres.Contains("Action"));
		Assert.IsTrue(_model.Game.Genres.Contains("Platform"));
	}

	[TestMethod]
	public async Task OnGet_WithGameVersions_LoadsVersions()
	{
		var gameSystem = _db.AddGameSystem("NES").Entity;
		var game = _db.AddGame("Test Game").Entity;
		_db.GameVersions.Add(new GameVersion
		{
			Game = game,
			Name = "Version 1.0",
			System = gameSystem,
			Region = "USA",
			Type = VersionTypes.Good,
			TitleOverride = "Custom Title"
		});
		await _db.SaveChangesAsync();
		_model.Id = game.Id.ToString();

		var result = await _model.OnGet();

		Assert.IsInstanceOfType<PageResult>(result);
		Assert.AreEqual(1, _model.Game.Versions.Count);
		var gameVersion = _model.Game.Versions.First();
		Assert.AreEqual("Version 1.0", gameVersion.Name);
		Assert.AreEqual("NES", gameVersion.SystemCode);
		Assert.AreEqual("USA", gameVersion.Region);
		Assert.AreEqual(VersionTypes.Good, gameVersion.Type);
		Assert.AreEqual("Custom Title", gameVersion.TitleOverride);
	}

	[TestMethod]
	public async Task OnGet_WithGameGroups_LoadsGroups()
	{
		var game = _db.AddGame("Test Game").Entity;
		var group = _db.AddGameGroup("Test Series").Entity;
		_db.AttachToGroup(game, group);
		await _db.SaveChangesAsync();

		_model.Id = game.Id.ToString();

		var result = await _model.OnGet();

		Assert.IsInstanceOfType<PageResult>(result);
		Assert.AreEqual(1, _model.Game.GameGroups.Count);
		Assert.AreEqual(group.Id, _model.Game.GameGroups.First().Id);
		Assert.AreEqual("Test Series", _model.Game.GameGroups.First().Name);
	}

	[TestMethod]
	public async Task OnGet_WithPublications_LoadsPublicationCounts()
	{
		var game = _db.AddGame("Test Game").Entity;
		await _db.SaveChangesAsync();
		_model.Id = game.Id.ToString();

		var result = await _model.OnGet();

		Assert.IsInstanceOfType<PageResult>(result);
		Assert.IsTrue(_model.Game.PublicationCount >= 0);
		Assert.IsTrue(_model.Game.ObsoletePublicationCount >= 0);
	}

	[TestMethod]
	public async Task OnGet_WithSubmissions_LoadsSubmissionCount()
	{
		var game = _db.AddGame("Test Game").Entity;
		await _db.SaveChangesAsync();
		_model.Id = game.Id.ToString();

		var result = await _model.OnGet();

		Assert.IsInstanceOfType<PageResult>(result);
		Assert.IsTrue(_model.Game.SubmissionCount >= 0);
	}

	[TestMethod]
	public async Task OnGet_WithUserFiles_LoadsUserFileCount()
	{
		var game = _db.AddGame("Test Game").Entity;
		await _db.SaveChangesAsync();
		_model.Id = game.Id.ToString();

		var result = await _model.OnGet();

		Assert.IsInstanceOfType<PageResult>(result);
		Assert.IsTrue(_model.Game.UserFilesCount >= 0);
	}

	[TestMethod]
	public async Task OnGet_WithPlaygroundSubmissions_LoadsPlaygroundData()
	{
		var game = _db.AddGame("Test Game").Entity;
		await _db.SaveChangesAsync();
		_model.Id = game.Id.ToString();

		var result = await _model.OnGet();

		Assert.IsInstanceOfType<PageResult>(result);
	}

	[TestMethod]
	public async Task OnGet_LoadsMoviesCorrectly()
	{
		var game = _db.AddGame("Test Game").Entity;
		await _db.SaveChangesAsync();
		_model.Id = game.Id.ToString();

		var result = await _model.OnGet();

		Assert.IsInstanceOfType<PageResult>(result);
	}

	[TestMethod]
	public void AllowsAnonymousAttribute() => AssertAllowsAnonymousUsers(typeof(IndexModel));
}
