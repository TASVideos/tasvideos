using TASVideos.Data.Entity.Game;
using TASVideos.Pages.Games;
using TASVideos.Tests.Base;

namespace TASVideos.RazorPages.Tests.Pages.Games;

[TestClass]
public class ListModelTests : TestDbBase
{
	private readonly ListModel _model;

	public ListModelTests()
	{
		_model = new ListModel(_db);
	}

	[TestMethod]
	public async Task OnGet_WithValidModel_LoadsGamesAndLists()
	{
		var gameSystem = _db.AddGameSystem("NES").Entity;
		_db.AddGenre("Action");
		_db.AddGameGroup("Test Series");
		var game = _db.AddGame("Test Game").Entity;
		_db.GameVersions.Add(new GameVersion { Game = game, System = gameSystem });
		await _db.SaveChangesAsync();

		await _model.OnGet();

		Assert.IsTrue(_model.SystemList.Count > 0);
		Assert.IsTrue(_model.LetterList.Count > 0);
	}

	[TestMethod]
	public async Task OnGet_WithInvalidModel_DoesNotLoadGames()
	{
		_model.SearchTerms = "ab";
		_model.ModelState.AddModelError("SearchTerms", "Search terms must be at least 3 characters.");

		await _model.OnGet();

		Assert.AreEqual(0, _model.Games.Count());
	}

	[TestMethod]
	public async Task OnGet_WithGenreFilter_FiltersGamesByGenre()
	{
		var actionGenre = _db.AddGenre("Action").Entity;
		var platformGenre = _db.AddGenre("Platform").Entity;
		var actionGame = _db.AddGame("Action Game").Entity;
		var platformGame = _db.AddGame("Platform Game").Entity;
		_db.AttachGenre(actionGame, actionGenre);
		_db.AttachGenre(platformGame, platformGenre);
		await _db.SaveChangesAsync();

		_model.Search = new ListModel.GameListRequest { Genre = "Action" };

		await _model.OnGet();

		Assert.IsTrue(_model.Games.Any(g => g.Name == "Action Game"));
		Assert.IsFalse(_model.Games.Any(g => g.Name == "Platform Game"));
	}

	[TestMethod]
	public async Task OnGet_WithGroupFilter_FiltersGamesByGroup()
	{
		var group1 = _db.AddGameGroup("Group 1").Entity;
		var group2 = _db.AddGameGroup("Group 2").Entity;
		var game1 = _db.AddGame("Game 1").Entity;
		var game2 = _db.AddGame("Game 2").Entity;
		_db.AttachToGroup(game1, group1);
		_db.AttachToGroup(game2, group2);
		await _db.SaveChangesAsync();

		_model.Search = new ListModel.GameListRequest { Group = "Group 1" };

		await _model.OnGet();

		Assert.IsTrue(_model.Games.Any(g => g.Name == "Game 1"));
		Assert.IsFalse(_model.Games.Any(g => g.Name == "Game 2"));
	}

	[TestMethod]
	public async Task OnGet_WithStartsWithFilter_FiltersGamesByFirstLetter()
	{
		_db.AddGame("Alpha Game");
		_db.AddGame("Beta Game");
		await _db.SaveChangesAsync();

		_model.Search = new ListModel.GameListRequest { StartsWith = "A" };

		await _model.OnGet();

		Assert.IsTrue(_model.Games.Any(g => g.Name == "Alpha Game"));
		Assert.IsFalse(_model.Games.Any(g => g.Name == "Beta Game"));
	}

	[TestMethod]
	public async Task OnGet_WithSearchTerms_FiltersGamesBySearchText()
	{
		_db.AddGame("Super Mario Bros");
		_db.AddGame("Sonic the Hedgehog");
		await _db.SaveChangesAsync();

		_model.Search = new ListModel.GameListRequest { SearchTerms = "Mario" };

		await _model.OnGet();

		Assert.IsTrue(_model.Games.Any(g => g.Name.Contains("Mario")));
	}

	[TestMethod]
	public async Task OnGet_LoadsSystemListWithAnyEntry()
	{
		_db.AddGameSystem("NES");
		await _db.SaveChangesAsync();

		await _model.OnGet();

		Assert.IsTrue(_model.SystemList.Any(s => s.Value == ""));
		Assert.IsTrue(_model.SystemList.Any(s => s.Value == "NES"));
	}

	[TestMethod]
	public async Task OnGet_LoadsLetterListWithDistinctFirstLetters()
	{
		_db.AddGame("Alpha Game");
		_db.AddGame("Beta Game");
		_db.AddGame("Another Alpha Game");
		await _db.SaveChangesAsync();

		await _model.OnGet();

		Assert.IsTrue(_model.LetterList.Any(l => l.Value == "A"));
		Assert.IsTrue(_model.LetterList.Any(l => l.Value == "B"));
		Assert.AreEqual(1, _model.LetterList.Count(l => l.Value == "A"));
	}

	[TestMethod]
	public async Task OnGet_LoadsGenreListWithDistinctGenres()
	{
		_db.AddGenre("Action");
		_db.AddGenre("Platform");
		_db.AddGenre("Action");
		await _db.SaveChangesAsync();

		await _model.OnGet();

		Assert.IsTrue(_model.GenreList.Any(g => g.Value == "Action"));
		Assert.IsTrue(_model.GenreList.Any(g => g.Value == "Platform"));
	}

	[TestMethod]
	public async Task OnGet_LoadsGroupListWithDistinctGroups()
	{
		_db.AddGameGroup("Series 1");
		_db.AddGameGroup("Series 2");
		await _db.SaveChangesAsync();

		await _model.OnGet();

		Assert.IsTrue(_model.GroupList.Any(g => g.Value == "Series 1"));
		Assert.IsTrue(_model.GroupList.Any(g => g.Value == "Series 2"));
	}

	[TestMethod]
	public void AllowsAnonymousAttribute() => AssertAllowsAnonymousUsers(typeof(ListModel));
}
