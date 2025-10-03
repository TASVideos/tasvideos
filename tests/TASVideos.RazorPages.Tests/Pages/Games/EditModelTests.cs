using TASVideos.Core.Services.ExternalMediaPublisher;
using TASVideos.Core.Services.Wiki;
using TASVideos.Core.Settings;
using TASVideos.Pages.Games;
using TASVideos.Services;

namespace TASVideos.RazorPages.Tests.Pages.Games;

[TestClass]
public class EditModelTests : BasePageModelTests
{
	private readonly IWikiPages _wikiPages;
	private readonly IExternalMediaPublisher _publisher;
	private readonly EditModel _model;

	public EditModelTests()
	{
		_wikiPages = Substitute.For<IWikiPages>();
		_publisher = Substitute.For<IExternalMediaPublisher>();
		_publisher.ToAbsolute(Arg.Any<string>()).Returns(x => "https://tasvideos.org" + x.Arg<string>());
		var settings = new AppSettings { BaseUrl = "https://tasvideos.org" };
		_model = new EditModel(_db, _wikiPages, _publisher, settings)
		{
			PageContext = TestPageContext()
		};
	}

	[TestMethod]
	public async Task OnGet_NewGame_InitializesNewGame()
	{
		var result = await _model.OnGet();
		Assert.IsInstanceOfType(result, typeof(PageResult));
	}

	[TestMethod]
	public async Task OnGet_NonExistentGame_ReturnsNotFound()
	{
		_model.Id = 999;
		var result = await _model.OnGet();
		Assert.IsInstanceOfType(result, typeof(NotFoundResult));
	}

	[TestMethod]
	public async Task OnGet_ExistingGame_LoadsGameData()
	{
		var genre1 = _db.AddGenre().Entity;
		var genre2 = _db.AddGenre("Platformer").Entity;
		var group1 = _db.AddGameGroup("Mario Series").Entity;

		var game = _db.AddGame("Test Game", "TG").Entity;
		game.Aliases = "TestAlias,GameAlias";
		game.ScreenshotUrl = "https://example.com/screenshot.png";
		game.GameResourcesPage = "TestGameResources";

		_db.AttachGenre(game, genre1);
		_db.AttachGenre(game, genre2);
		_db.AttachToGroup(game, group1);
		await _db.SaveChangesAsync();

		_model.Id = game.Id;

		var result = await _model.OnGet();

		Assert.IsInstanceOfType(result, typeof(PageResult));
		Assert.AreEqual("Test Game", _model.Game.DisplayName);
		Assert.AreEqual("TG", _model.Game.Abbreviation);
		Assert.AreEqual("TestAlias,GameAlias", _model.Game.Aliases);
		Assert.AreEqual("https://example.com/screenshot.png", _model.Game.ScreenshotUrl);
		Assert.AreEqual("TestGameResources", _model.Game.GameResourcesPage);
		Assert.AreEqual(2, _model.Game.Genres.Count);
		Assert.IsTrue(_model.Game.Genres.Contains(genre1.Id));
		Assert.IsTrue(_model.Game.Genres.Contains(genre2.Id));
		Assert.AreEqual(1, _model.Game.Groups.Count);
		Assert.IsTrue(_model.Game.Groups.Contains(group1.Id));

		Assert.IsTrue(_model.AvailableGenres.Any(g => g.Text == "Action"));
		Assert.IsTrue(_model.AvailableGenres.Any(g => g.Text == "Platformer"));
		Assert.IsTrue(_model.AvailableGroups.Any(g => g.Text == "Mario Series"));
	}

	[TestMethod]
	public async Task OnGet_GameWithoutSubmissionsOrPublications_CanDelete()
	{
		var game = _db.AddGame("Deletable Game").Entity;
		await _db.SaveChangesAsync();
		_model.Id = game.Id;

		await _model.OnGet();

		Assert.IsTrue(_model.CanDelete);
	}

	[TestMethod]
	public async Task OnGet_GameWithSubmissions_CannotDelete()
	{
		var game = _db.AddGame("Game With Submissions").Entity;
		var sub = _db.AddAndSaveUnpublishedSubmission().Entity;
		sub.Game = game;
		await _db.SaveChangesAsync();

		_model.Id = game.Id;

		await _model.OnGet();

		Assert.IsFalse(_model.CanDelete);
	}

	[TestMethod]
	public async Task OnPost_InvalidModelState_ReturnsPageWithData()
	{
		_db.AddGenre();
		await _db.SaveChangesAsync();
		_model.ModelState.AddModelError("DisplayName", "DisplayName is required");

		var result = await _model.OnPost();

		Assert.IsInstanceOfType(result, typeof(PageResult));
		Assert.IsTrue(_model.AvailableGenres.Any());
	}

	[TestMethod]
	public async Task OnPost_GameResourcesPageNotFound_AddsModelError()
	{
		_db.AddGenre();
		await _db.SaveChangesAsync();

		_wikiPages.Page("NonExistentPage").Returns((IWikiPage?)null);

		_model.Game = new EditModel.GameEdit
		{
			DisplayName = "Test Game",
			GameResourcesPage = "NonExistentPage"
		};

		var result = await _model.OnPost();

		Assert.IsInstanceOfType(result, typeof(PageResult));
		Assert.IsTrue(_model.ModelState.ContainsKey($"{nameof(_model.Game)}.{nameof(_model.Game.GameResourcesPage)}"));
	}

	[TestMethod]
	public async Task OnPost_DuplicateAbbreviation_AddsModelError()
	{
		_db.AddGame("Existing Game", "EG");
		await _db.SaveChangesAsync();

		_model.Game = new EditModel.GameEdit
		{
			DisplayName = "New Game",
			Abbreviation = "EG" // Same as existing
		};

		var result = await _model.OnPost();

		Assert.IsInstanceOfType(result, typeof(PageResult));
		Assert.IsTrue(_model.ModelState.ContainsKey($"{nameof(_model.Game)}.{nameof(_model.Game.Abbreviation)}"));
	}

	[TestMethod]
	public async Task OnPost_CreateNewGame_CreatesGameAndGoal()
	{
		var genre = _db.AddGenre().Entity;
		var group = _db.AddGameGroup("Test Series").Entity;
		await _db.SaveChangesAsync();

		_model.Game = new EditModel.GameEdit
		{
			DisplayName = "New Game",
			Abbreviation = "NG",
			Aliases = "NewGameAlias",
			ScreenshotUrl = "https://example.com/new.png",
			GameResourcesPage = "NewGameResources",
			Genres = [genre.Id],
			Groups = [group.Id]
		};

		_wikiPages.Page("NewGameResources").Returns(new WikiResult { PageName = "NewGameResources" });

		var result = await _model.OnPost();

		AssertRedirect(result, "Index");

		var createdGame = await _db.Games
			.Include(g => g.GameGenres)
			.Include(g => g.GameGroups)
			.SingleOrDefaultAsync(g => g.DisplayName == "New Game");
		Assert.IsNotNull(createdGame);
		Assert.AreEqual("New Game", createdGame.DisplayName);
		Assert.AreEqual("NG", createdGame.Abbreviation);
		Assert.AreEqual("NewGameAlias", createdGame.Aliases);
		Assert.AreEqual("https://example.com/new.png", createdGame.ScreenshotUrl);
		Assert.AreEqual("NewGameResources", createdGame.GameResourcesPage);
		Assert.AreEqual(1, createdGame.GameGenres.Count);
		Assert.AreEqual(genre.Id, createdGame.GameGenres.First().GenreId);
		Assert.AreEqual(1, createdGame.GameGroups.Count);
		Assert.AreEqual(group.Id, createdGame.GameGroups.First().GameGroupId);

		// Verify baseline goal was created
		var goal = await _db.GameGoals.SingleOrDefaultAsync(g => g.GameId == createdGame.Id);
		Assert.IsNotNull(goal);
		Assert.AreEqual("baseline", goal.DisplayName);
	}

	[TestMethod]
	public async Task OnPost_UpdateExistingGame_UpdatesAllFields()
	{
		var genre1 = _db.AddGenre().Entity;
		var genre2 = _db.AddGenre("RPG").Entity;
		var group1 = _db.AddGameGroup("Old Series").Entity;
		var group2 = _db.AddGameGroup("New Series").Entity;

		var game = _db.AddGame("Original Game", "OG").Entity;
		game.Aliases = "OriginalAlias";
		game.ScreenshotUrl = "https://example.com/original.png";
		game.GameResourcesPage = "OriginalResources";

		_db.AttachGenre(game, genre1);
		_db.AttachToGroup(game, group1);
		await _db.SaveChangesAsync();

		_model.Id = game.Id;
		_model.Game = new EditModel.GameEdit
		{
			DisplayName = "Updated Game",
			Abbreviation = "UG",
			Aliases = "UpdatedAlias",
			ScreenshotUrl = "https://example.com/updated.png",
			GameResourcesPage = "UpdatedResources",
			Genres = [genre2.Id],
			Groups = [group2.Id]
		};

		_wikiPages.Page("UpdatedResources").Returns(new WikiResult { PageName = "UpdatedResources" });

		var result = await _model.OnPost();

		AssertRedirect(result, "Index");

		var updatedGame = await _db.Games
			.Include(g => g.GameGenres)
			.Include(g => g.GameGroups)
			.SingleOrDefaultAsync(g => g.Id == game.Id);

		Assert.IsNotNull(updatedGame);
		Assert.AreEqual("Updated Game", updatedGame.DisplayName);
		Assert.AreEqual("UG", updatedGame.Abbreviation);
		Assert.AreEqual("UpdatedAlias", updatedGame.Aliases);
		Assert.AreEqual("https://example.com/updated.png", updatedGame.ScreenshotUrl);
		Assert.AreEqual("UpdatedResources", updatedGame.GameResourcesPage);
		Assert.AreEqual(1, updatedGame.GameGenres.Count);
		Assert.AreEqual(genre2.Id, updatedGame.GameGenres.First().GenreId);
		Assert.AreEqual(1, updatedGame.GameGroups.Count);
		Assert.AreEqual(group2.Id, updatedGame.GameGroups.First().GameGroupId);

		await _publisher.Received(1).Send(Arg.Any<Post>());
	}

	[TestMethod]
	public async Task OnPost_GameResourcesPageProcessing_RemovesBaseUrl()
	{
		_model.Game = new EditModel.GameEdit
		{
			DisplayName = "Test Game",
			GameResourcesPage = "https://tasvideos.org/GameResources/TestGame/"
		};

		_wikiPages.Page("GameResources/TestGame").Returns(new WikiResult { PageName = "GameResources/TestGame" });

		await _model.OnPost();

		var createdGame = await _db.Games.SingleOrDefaultAsync();
		Assert.IsNotNull(createdGame);
		Assert.AreEqual("GameResources/TestGame", createdGame.GameResourcesPage);
	}

	[TestMethod]
	public async Task OnPost_AliasesProcessing_RemovesExtraSpaces()
	{
		_model.Game = new EditModel.GameEdit
		{
			DisplayName = "Test Game",
			Aliases = "Alias1, Alias2, Alias3"
		};

		await _model.OnPost();

		var createdGame = await _db.Games.SingleOrDefaultAsync();
		Assert.IsNotNull(createdGame);
		Assert.AreEqual("Alias1,Alias2,Alias3", createdGame.Aliases);
	}

	[TestMethod]
	public async Task OnPost_DatabaseSaveFailure_HandlesGracefully()
	{
		_db.CreateUpdateConflict();
		_model.Game = new EditModel.GameEdit
		{
			DisplayName = "Test Game",
			Abbreviation = "TG"
		};

		var result = await _model.OnPost();

		AssertRedirect(result, "Index");
	}

	[TestMethod]
	public async Task OnPostDelete_NoId_ReturnsNotFound()
	{
		var result = await _model.OnPostDelete();
		Assert.IsInstanceOfType(result, typeof(NotFoundResult));
	}

	[TestMethod]
	public async Task OnPostDelete_WithoutDeletePermission_ReturnsAccessDenied()
	{
		var user = _db.AddUser("RegularUser").Entity;
		AddAuthenticatedUser(_model, user, []); // No delete permission

		var game = _db.AddGame().Entity;
		await _db.SaveChangesAsync();

		_model.Id = game.Id;

		var result = await _model.OnPostDelete();

		AssertAccessDenied(result);
	}

	[TestMethod]
	public async Task OnPostDelete_GameWithSubmissions_ShowsErrorAndRedirects()
	{
		var user = _db.AddUser("AdminUser").Entity;
		AddAuthenticatedUser(_model, user, [PermissionTo.DeleteGameEntries]);

		var gameSystem = _db.AddGameSystem("NES").Entity;
		var game = _db.AddGame("Game With Submissions").Entity;
		_db.Submissions.Add(new Submission { Submitter = user, System = gameSystem, Game = game });
		await _db.SaveChangesAsync();

		_model.Id = game.Id;

		var result = await _model.OnPostDelete();

		AssertRedirect(result, "List");
		Assert.IsTrue(_db.Games.Any(g => g.Id == game.Id));
	}

	[TestMethod]
	public async Task OnPostDelete_NonExistentGame_ReturnsNotFound()
	{
		var user = _db.AddUser("AdminUser").Entity;
		AddAuthenticatedUser(_model, user, [PermissionTo.DeleteGameEntries]);
		_model.Id = 999;

		var result = await _model.OnPostDelete();

		Assert.IsInstanceOfType(result, typeof(NotFoundResult));
	}

	[TestMethod]
	public async Task OnPostDelete_ValidGame_DeletesSuccessfully()
	{
		var user = _db.AddUser("AdminUser").Entity;
		AddAuthenticatedUser(_model, user, [PermissionTo.DeleteGameEntries]);

		var game = _db.AddGame("Deletable Game").Entity;
		await _db.SaveChangesAsync();

		_model.Id = game.Id;

		var result = await _model.OnPostDelete();

		AssertRedirect(result, "List");
		Assert.IsFalse(_db.Games.Any(g => g.Id == game.Id));
	}

	[TestMethod]
	public async Task OnPostDelete_SuccessfulDelete_SendsMessage()
	{
		var user = _db.AddUser("AdminUser").Entity;
		AddAuthenticatedUser(_model, user, [PermissionTo.DeleteGameEntries]);

		var game = _db.AddGame("Test Game").Entity;
		await _db.SaveChangesAsync();

		_model.Id = game.Id;

		await _model.OnPostDelete();

		await _publisher.Received(1).Send(Arg.Any<Post>());
	}

	[TestMethod]
	public void RequiresPermission() => AssertHasPermission(typeof(EditModel), PermissionTo.CatalogMovies);
}
