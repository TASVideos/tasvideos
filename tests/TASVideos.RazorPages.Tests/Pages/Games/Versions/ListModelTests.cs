using TASVideos.Data.Entity.Game;
using TASVideos.Pages.Games.Versions;
using TASVideos.Tests.Base;

namespace TASVideos.RazorPages.Tests.Pages.Games.Versions;

[TestClass]
public class ListModelTests : TestDbBase
{
	private readonly ListModel _model;

	public ListModelTests()
	{
		_model = new ListModel(_db);
	}

	[TestMethod]
	public async Task OnGet_GameNotFound_ReturnsNotFound()
	{
		_model.GameId = 999;
		var result = await _model.OnGet();
		Assert.IsInstanceOfType<NotFoundResult>(result);
	}

	[TestMethod]
	public async Task OnGet_GameExists_SetsGameDisplayName()
	{
		var game = _db.AddGame("Test Game").Entity;
		await _db.SaveChangesAsync();
		_model.GameId = game.Id;

		var result = await _model.OnGet();

		Assert.IsInstanceOfType<PageResult>(result);
		Assert.AreEqual("Test Game", _model.GameDisplayName);
		Assert.AreEqual(0, _model.Versions.Count);
	}

	[TestMethod]
	public async Task OnGet_GameExistsWithVersions_LoadsVersions()
	{
		var game = _db.AddGame("Test Game").Entity;
		var system = _db.AddGameSystem("NES").Entity;
		var version1 = new GameVersion
		{
			Game = game,
			Name = "Version 1",
			Md5 = "test-md5-1",
			Sha1 = "test-sha1-1",
			Version = "1.0",
			Region = "USA",
			Type = VersionTypes.Good,
			System = system,
			TitleOverride = "Override Title",
			SourceDb = "GoodNES"
		};
		var version2 = new GameVersion
		{
			Game = game,
			Name = "Version 2",
			Md5 = "test-md5-2",
			Sha1 = "test-sha1-2",
			Version = "1.1",
			Region = "Europe",
			Type = VersionTypes.Hack,
			System = system,
			TitleOverride = null,
			SourceDb = "No-Intro"
		};
		_db.GameVersions.AddRange(version1, version2);
		await _db.SaveChangesAsync();
		_model.GameId = game.Id;

		var result = await _model.OnGet();

		Assert.IsInstanceOfType<PageResult>(result);
		Assert.AreEqual(2, _model.Versions.Count);

		var firstVersion = _model.Versions.First(v => v.Name == "Version 1");
		Assert.AreEqual(version1.Id, firstVersion.Id);
		Assert.AreEqual("test-md5-1", firstVersion.Md5);
		Assert.AreEqual("test-sha1-1", firstVersion.Sha1);
		Assert.AreEqual("1.0", firstVersion.Version);
		Assert.AreEqual("USA", firstVersion.Region);
		Assert.AreEqual(VersionTypes.Good, firstVersion.Type);
		Assert.AreEqual("NES", firstVersion.System);
		Assert.AreEqual("Override Title", firstVersion.TitleOverride);
		Assert.AreEqual("GoodNES", firstVersion.SourceDb);

		var secondVersion = _model.Versions.First(v => v.Name == "Version 2");
		Assert.AreEqual(version2.Id, secondVersion.Id);
		Assert.AreEqual("test-md5-2", secondVersion.Md5);
		Assert.AreEqual("test-sha1-2", secondVersion.Sha1);
		Assert.AreEqual("1.1", secondVersion.Version);
		Assert.AreEqual("Europe", secondVersion.Region);
		Assert.AreEqual(VersionTypes.Hack, secondVersion.Type);
		Assert.AreEqual("NES", secondVersion.System);
		Assert.IsNull(secondVersion.TitleOverride);
		Assert.AreEqual("No-Intro", secondVersion.SourceDb);
	}

	[TestMethod]
	public async Task OnGet_GameExistsWithVersionsFromDifferentGames_OnlyLoadsVersionsForSpecifiedGame()
	{
		var game1 = _db.AddGame("Game 1").Entity;
		var game2 = _db.AddGame("Game 2").Entity;
		var system = _db.AddGameSystem("NES").Entity;
		var version1 = new GameVersion { Game = game1, Name = "Game 1 Version", System = system };
		var version2 = new GameVersion { Game = game2, Name = "Game 2 Version", System = system };
		_db.GameVersions.AddRange(version1, version2);
		await _db.SaveChangesAsync();
		_model.GameId = game1.Id;

		var result = await _model.OnGet();

		Assert.IsInstanceOfType<PageResult>(result);
		Assert.AreEqual(1, _model.Versions.Count);
		Assert.AreEqual("Game 1 Version", _model.Versions.Single().Name);
	}
}
