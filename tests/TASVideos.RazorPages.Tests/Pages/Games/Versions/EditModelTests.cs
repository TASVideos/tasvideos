using Microsoft.AspNetCore.Http;
using TASVideos.Core.Services.ExternalMediaPublisher;
using TASVideos.Data.Entity.Game;
using TASVideos.Pages.Games.Versions;
using TASVideos.Services;
using TASVideos.Tests.Base;

namespace TASVideos.RazorPages.Tests.Pages.Games.Versions;

[TestClass]
public class EditModelTests : TestDbBase
{
	private readonly EditModel _model;
	private readonly IExternalMediaPublisher _publisher;

	public EditModelTests()
	{
		_publisher = Substitute.For<IExternalMediaPublisher>();
		_model = new EditModel(_db, _publisher)
		{
			PageContext = new PageContext(new ActionContext(new DefaultHttpContext(), new(), new()))
		};
	}

	[TestMethod]
	public async Task OnGet_GameNotFound_ReturnsNotFound()
	{
		_model.GameId = 999;
		var result = await _model.OnGet();
		Assert.IsInstanceOfType<NotFoundResult>(result);
	}

	[TestMethod]
	public async Task OnGet_GameExistsNoVersionId_SetsGameNameAndReturnsPage()
	{
		var game = _db.AddGame("Test Game").Entity;
		_db.AddGameSystem("NES");
		await _db.SaveChangesAsync();
		_model.GameId = game.Id;

		var result = await _model.OnGet();

		Assert.IsInstanceOfType<PageResult>(result);
		Assert.AreEqual("Test Game", _model.GameName);
		Assert.IsTrue(_model.AvailableSystems.Count > 0);
	}

	[TestMethod]
	public async Task OnGet_GameExistsWithSystemId_SetsSystemInVersion()
	{
		var game = _db.AddGame("Test Game").Entity;
		var system = _db.AddGameSystem("NES").Entity;
		await _db.SaveChangesAsync();
		_model.GameId = game.Id;
		_model.SystemId = system.Id;

		var result = await _model.OnGet();

		Assert.IsInstanceOfType<PageResult>(result);
		Assert.AreEqual("NES", _model.Version.System);
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
	public async Task OnGet_ValidVersion_LoadsVersionData()
	{
		var game = _db.AddGame("Test Game").Entity;
		var system = _db.AddGameSystem("NES").Entity;
		var version = _db.GameVersions.Add(new GameVersion
		{
			Game = game,
			Name = "Test Version",
			Md5 = "abcdef1234567890abcdef1234567890",
			Sha1 = "abcdef1234567890abcdef1234567890abcdef12",
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
		Assert.AreEqual("NES", _model.Version.System);
		Assert.AreEqual("Test Version", _model.Version.Name);
		Assert.AreEqual("abcdef1234567890abcdef1234567890", _model.Version.Md5);
		Assert.AreEqual("abcdef1234567890abcdef1234567890abcdef12", _model.Version.Sha1);
		Assert.AreEqual("1.0", _model.Version.Version);
		Assert.AreEqual("USA", _model.Version.Region);
		Assert.AreEqual(VersionTypes.Good, _model.Version.Type);
		Assert.AreEqual("Override Title", _model.Version.TitleOverride);
		Assert.AreEqual("GoodNES", _model.Version.SourceDb);
		Assert.AreEqual("Test notes", _model.Version.Notes);
	}

	[TestMethod]
	public async Task OnGet_VersionWithNullRegion_SetsEmptyRegion()
	{
		var game = _db.AddGame("Test Game").Entity;
		var system = _db.AddGameSystem("NES").Entity;
		var version = _db.GameVersions.Add(new GameVersion
		{
			Game = game,
			Name = "Test Version",
			Region = null,
			System = system
		}).Entity;
		await _db.SaveChangesAsync();
		_model.GameId = game.Id;
		_model.Id = version.Id;

		var result = await _model.OnGet();

		Assert.IsInstanceOfType<PageResult>(result);
		Assert.AreEqual("", _model.Version.Region);
	}

	[TestMethod]
	public async Task OnPost_InvalidModelState_ReturnsPageWithErrors()
	{
		var game = _db.AddGame("Test Game").Entity;
		_db.AddGameSystem("NES");
		await _db.SaveChangesAsync();
		_model.GameId = game.Id;
		_model.Version.System = "NES";
		_model.ModelState.AddModelError("Name", "Name is required");

		var result = await _model.OnPost();

		Assert.IsInstanceOfType<PageResult>(result);
		Assert.IsTrue(_model.AvailableSystems.Count > 0);
	}

	[TestMethod]
	public async Task OnPost_InvalidSystem_ReturnsBadRequest()
	{
		var game = _db.AddGame("Test Game").Entity;
		await _db.SaveChangesAsync();
		_model.GameId = game.Id;
		_model.Version = new EditModel.VersionEdit
		{
			System = "INVALID",
			Name = "Test Version"
		};

		var result = await _model.OnPost();

		Assert.IsInstanceOfType<BadRequestResult>(result);
	}

	[TestMethod]
	public async Task OnPost_CreateNewVersion_AddsVersionToDatabase()
	{
		var game = _db.AddGame("Test Game").Entity;
		var system = _db.AddGameSystem("NES").Entity;
		await _db.SaveChangesAsync();
		_model.GameId = game.Id;
		_model.Version = new EditModel.VersionEdit
		{
			System = "NES",
			Name = "New Version",
			Md5 = "abcdef1234567890abcdef1234567890",
			Sha1 = "abcdef1234567890abcdef1234567890abcdef12",
			Version = "1.0",
			Region = "USA",
			Type = VersionTypes.Good,
			TitleOverride = "Override",
			SourceDb = "GoodNES",
			Notes = "Notes"
		};

		var result = await _model.OnPost();

		Assert.IsInstanceOfType<RedirectToPageResult>(result);
		var createdVersion = await _db.GameVersions.SingleOrDefaultAsync(v => v.Name == "New Version");
		Assert.IsNotNull(createdVersion);
		Assert.AreEqual(game.Id, createdVersion.GameId);
		Assert.AreEqual(system.Id, createdVersion.SystemId);
		Assert.AreEqual("abcdef1234567890abcdef1234567890", createdVersion.Md5);
		Assert.AreEqual("abcdef1234567890abcdef1234567890abcdef12", createdVersion.Sha1);
		Assert.AreEqual("1.0", createdVersion.Version);
		Assert.AreEqual("USA", createdVersion.Region);
		Assert.AreEqual(VersionTypes.Good, createdVersion.Type);
		Assert.AreEqual("Override", createdVersion.TitleOverride);
		Assert.AreEqual("GoodNES", createdVersion.SourceDb);
		Assert.AreEqual("Notes", createdVersion.Notes);
		await _publisher.Received(1).Send(Arg.Any<Post>());
	}

	[TestMethod]
	public async Task OnPost_UpdateExistingVersion_UpdatesVersionInDatabase()
	{
		var game = _db.AddGame("Test Game").Entity;
		var system = _db.AddGameSystem("NES").Entity;
		var version = _db.GameVersions.Add(new GameVersion
		{
			Game = game,
			Name = "Original Version",
			System = system
		}).Entity;
		await _db.SaveChangesAsync();
		_model.GameId = game.Id;
		_model.Id = version.Id;
		_model.Version = new EditModel.VersionEdit
		{
			System = "NES",
			Name = "Updated Version",
			Md5 = "abcdef1234567890abcdef1234567890",
			Region = "Europe",
			Type = VersionTypes.Hack
		};

		var result = await _model.OnPost();

		Assert.IsInstanceOfType<RedirectToPageResult>(result);
		var updatedVersion = await _db.GameVersions.SingleOrDefaultAsync(v => v.Id == version.Id);
		Assert.IsNotNull(updatedVersion);
		Assert.AreEqual("Updated Version", updatedVersion.Name);
		Assert.AreEqual("abcdef1234567890abcdef1234567890", updatedVersion.Md5);
		Assert.AreEqual("Europe", updatedVersion.Region);
		Assert.AreEqual(VersionTypes.Hack, updatedVersion.Type);
		await _publisher.Received(1).Send(Arg.Any<Post>());
	}

	[TestMethod]
	public async Task OnPost_DatabaseSaveFailure_ReturnsPageWithError()
	{
		var game = _db.AddGame("Test Game").Entity;
		_db.AddGameSystem("NES");
		await _db.SaveChangesAsync();
		_model.GameId = game.Id;
		_model.Version = new EditModel.VersionEdit
		{
			System = "NES",
			Name = "Test Version"
		};
		_db.CreateUpdateConflict();

		var result = await _model.OnPost();

		Assert.IsInstanceOfType<PageResult>(result);
		Assert.IsTrue(_model.ModelState.ErrorCount > 0);
	}

	[TestMethod]
	public async Task OnPostDelete_NoId_ReturnsNotFound()
	{
		_model.GameId = 1;
		var result = await _model.OnPostDelete();
		Assert.IsInstanceOfType<NotFoundResult>(result);
	}

	[TestMethod]
	public async Task OnPostDelete_VersionInUse_ReturnsRedirectWithError()
	{
		var game = _db.AddGame("Test Game").Entity;
		var system = _db.AddGameSystem("NES").Entity;
		var version = _db.GameVersions.Add(new GameVersion { Game = game, Name = "Version", System = system }).Entity;
		await _db.SaveChangesAsync();

		var publication = _db.AddPublication().Entity;
		publication.GameVersionId = version.Id;
		await _db.SaveChangesAsync();

		_model.GameId = game.Id;
		_model.Id = version.Id;

		var result = await _model.OnPostDelete();

		Assert.IsInstanceOfType<RedirectToPageResult>(result);
		var redirect = (RedirectToPageResult)result;
		Assert.AreEqual("List", redirect.PageName);
	}

	[TestMethod]
	public async Task OnPostDelete_ValidVersion_DeletesVersionFromDatabase()
	{
		var game = _db.AddGame("Test Game").Entity;
		var system = _db.AddGameSystem("NES").Entity;
		var version = _db.GameVersions.Add(new GameVersion { Game = game, Name = "Version", System = system }).Entity;
		await _db.SaveChangesAsync();
		_db.Entry(version).State = EntityState.Detached;
		_model.GameId = game.Id;
		_model.Id = version.Id;

		var result = await _model.OnPostDelete();

		Assert.IsInstanceOfType<RedirectToPageResult>(result);
		var deletedVersion = await _db.GameVersions.SingleOrDefaultAsync(v => v.Id == version.Id);
		Assert.IsNull(deletedVersion);
		await _publisher.Received(1).Send(Arg.Any<Post>());
	}

	[TestMethod]
	public async Task CanDelete_VersionWithSubmissions_ReturnsFalse()
	{
		var submitter = _db.AddUser("TestUser").Entity;
		var game = _db.AddGame("Test Game").Entity;
		var system = _db.AddGameSystem("NES").Entity;
		var version = _db.GameVersions.Add(new GameVersion { Game = game, Name = "Version", System = system }).Entity;
		await _db.SaveChangesAsync();

		_db.Submissions.Add(new Submission { GameVersionId = version.Id, Submitter = submitter, Title = "Test" });
		await _db.SaveChangesAsync();

		_model.GameId = game.Id;
		_model.Id = version.Id;
		var result = await _model.OnGet();

		Assert.IsInstanceOfType<PageResult>(result);
		Assert.IsFalse(_model.CanDelete);
	}

	[TestMethod]
	public async Task CanDelete_VersionWithPublications_ReturnsFalse()
	{
		var game = _db.AddGame("Test Game").Entity;
		var system = _db.AddGameSystem("NES").Entity;
		var version = _db.GameVersions.Add(new GameVersion { Game = game, Name = "Version", System = system }).Entity;
		await _db.SaveChangesAsync();

		var publication = _db.AddPublication().Entity;
		publication.GameVersionId = version.Id;
		await _db.SaveChangesAsync();

		_model.GameId = game.Id;
		_model.Id = version.Id;
		var result = await _model.OnGet();

		Assert.IsInstanceOfType<PageResult>(result);
		Assert.IsFalse(_model.CanDelete);
	}

	[TestMethod]
	public async Task CanDelete_VersionWithoutReferences_ReturnsTrue()
	{
		var game = _db.AddGame("Test Game").Entity;
		var system = _db.AddGameSystem("NES").Entity;
		var version = _db.GameVersions.Add(new GameVersion { Game = game, Name = "Version", System = system }).Entity;
		await _db.SaveChangesAsync();

		_model.GameId = game.Id;
		_model.Id = version.Id;
		var result = await _model.OnGet();

		Assert.IsInstanceOfType<PageResult>(result);
		Assert.IsTrue(_model.CanDelete);
	}
}
