using TASVideos.Data.Entity.Game;
using TASVideos.Pages.Games.Versions;
using TASVideos.Tests.Base;

namespace TASVideos.RazorPages.Tests.Pages.Games.Versions;

[TestClass]
public class ViewModelTests : TestDbBase
{
	private readonly ViewModel _model;

	public ViewModelTests()
	{
		_model = new ViewModel(_db);
	}

	[TestMethod]
	public async Task OnGet_GameNotFound_ReturnsNotFound()
	{
		_model.GameId = 999;
		_model.Id = 1;

		var result = await _model.OnGet();

		Assert.IsInstanceOfType<NotFoundResult>(result);
	}

	[TestMethod]
	public async Task OnGet_VersionNotFound_ReturnsNotFound()
	{
		var game = _db.AddGame("Test Game").Entity;
		await _db.SaveChangesAsync();
		_model.GameId = game.Id;
		_model.Id = 999;

		var result = await _model.OnGet();

		Assert.IsInstanceOfType<NotFoundResult>(result);
	}

	[TestMethod]
	public async Task OnGet_VersionNotMatchingGame_ReturnsNotFound()
	{
		var game1 = _db.AddGame("Game 1").Entity;
		var game2 = _db.AddGame("Game 2").Entity;
		var system = _db.AddGameSystem("NES").Entity;
		var version = _db.GameVersions.Add(new GameVersion { Game = game2, Name = "Version", System = system }).Entity;
		await _db.SaveChangesAsync();
		_model.GameId = game1.Id;
		_model.Id = version.Id;

		var result = await _model.OnGet();

		Assert.IsInstanceOfType<NotFoundResult>(result);
	}

	[TestMethod]
	public async Task OnGet_ValidGameAndVersion_LoadsData()
	{
		var game = _db.AddGame("Test Game").Entity;
		var system = _db.AddGameSystem("NES").Entity;
		var version = _db.GameVersions.Add(new GameVersion
		{
			Game = game,
			Name = "Test Version",
			Md5 = "test-md5",
			Sha1 = "test-sha1",
			Version = "1.0",
			Region = "USA",
			Type = VersionTypes.Good,
			System = system,
			TitleOverride = "Override Title",
			SourceDb = "GoodNES",
			Notes = "Test notes"
		}).Entity;
		await _db.SaveChangesAsync();
		_model.GameId = game.Id;
		_model.Id = version.Id;

		var result = await _model.OnGet();

		Assert.IsInstanceOfType<PageResult>(result);
		Assert.AreEqual("Test Game", _model.Game);
		Assert.AreEqual(version.Id, _model.Version.Id);
		Assert.AreEqual("NES", _model.Version.SystemCode);
		Assert.AreEqual("Test Version", _model.Version.Name);
		Assert.AreEqual("test-md5", _model.Version.Md5);
		Assert.AreEqual("test-sha1", _model.Version.Sha1);
		Assert.AreEqual("1.0", _model.Version.Version);
		Assert.AreEqual("USA", _model.Version.Region);
		Assert.AreEqual(VersionTypes.Good, _model.Version.Type);
		Assert.AreEqual("Override Title", _model.Version.TitleOverride);
		Assert.AreEqual("GoodNES", _model.Version.SourceDb);
		Assert.AreEqual("Test notes", _model.Version.Notes);
	}

	[TestMethod]
	public async Task OnGet_LoadsPublicationsForVersion()
	{
		var game = _db.AddGame("Test Game").Entity;
		var system = _db.AddGameSystem("NES").Entity;
		var version = _db.GameVersions.Add(new GameVersion { Game = game, Name = "Test Version", System = system }).Entity;
		await _db.SaveChangesAsync();

		var publication1 = _db.AddPublication().Entity;
		publication1.Title = "Publication 1";
		publication1.GameVersionId = version.Id;

		var publication2 = _db.AddPublication().Entity;
		publication2.Title = "Publication 2";
		publication2.GameVersionId = version.Id;

		var otherVersion = _db.GameVersions.Add(new GameVersion { Game = game, Name = "Other Version", System = system }).Entity;
		await _db.SaveChangesAsync();

		var otherPublication = _db.AddPublication().Entity;
		otherPublication.Title = "Other Publication";
		otherPublication.GameVersionId = otherVersion.Id;

		await _db.SaveChangesAsync();
		_model.GameId = game.Id;
		_model.Id = version.Id;

		var result = await _model.OnGet();

		Assert.IsInstanceOfType<PageResult>(result);
		Assert.AreEqual(2, _model.Publications.Count);
		Assert.IsTrue(_model.Publications.Any(p => p.Title == "Publication 1"));
		Assert.IsTrue(_model.Publications.Any(p => p.Title == "Publication 2"));
		Assert.IsFalse(_model.Publications.Any(p => p.Title == "Other Publication"));
	}

	[TestMethod]
	public async Task OnGet_LoadsSubmissionsForVersion()
	{
		var submitter = _db.AddUser("TestUser").Entity;
		var game = _db.AddGame("Test Game").Entity;
		var system = _db.AddGameSystem("NES").Entity;
		var version = _db.GameVersions.Add(new GameVersion { Game = game, Name = "Test Version", System = system }).Entity;
		await _db.SaveChangesAsync();

		var submission1 = new Submission { Title = "Submission 1", GameVersionId = version.Id, Submitter = submitter };
		var submission2 = new Submission { Title = "Submission 2", GameVersionId = version.Id, Submitter = submitter };
		_db.Submissions.AddRange(submission1, submission2);

		var otherVersion = _db.GameVersions.Add(new GameVersion { Game = game, Name = "Other Version", System = system }).Entity;
		await _db.SaveChangesAsync();
		_db.Submissions.Add(new Submission { Title = "Other Submission", GameVersionId = otherVersion.Id, Submitter = submitter });

		await _db.SaveChangesAsync();
		_model.GameId = game.Id;
		_model.Id = version.Id;

		var result = await _model.OnGet();

		Assert.IsInstanceOfType<PageResult>(result);
		Assert.AreEqual(2, _model.Submissions.Count);
		Assert.IsTrue(_model.Submissions.Any(s => s.Title == "Submission 1"));
		Assert.IsTrue(_model.Submissions.Any(s => s.Title == "Submission 2"));
		Assert.IsFalse(_model.Submissions.Any(s => s.Title == "Other Submission"));
	}

	[TestMethod]
	public async Task OnGet_NoPublicationsOrSubmissions_ReturnsEmptyLists()
	{
		var game = _db.AddGame("Test Game").Entity;
		var system = _db.AddGameSystem("NES").Entity;
		var version = _db.GameVersions.Add(new GameVersion { Game = game, Name = "Test Version", System = system }).Entity;
		await _db.SaveChangesAsync();
		_model.GameId = game.Id;
		_model.Id = version.Id;

		var result = await _model.OnGet();

		Assert.IsInstanceOfType<PageResult>(result);
		Assert.AreEqual(0, _model.Publications.Count);
		Assert.AreEqual(0, _model.Submissions.Count);
	}
}
