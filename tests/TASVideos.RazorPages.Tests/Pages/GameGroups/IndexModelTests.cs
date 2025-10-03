using TASVideos.Pages.GameGroups;

namespace TASVideos.RazorPages.Tests.Pages.GameGroups;

[TestClass]
public class IndexModelTests : BasePageModelTests
{
	private readonly IndexModel _model;

	public IndexModelTests()
	{
		_model = new IndexModel(_db)
		{
			PageContext = TestPageContext()
		};
	}

	[TestMethod]
	public async Task OnGet_GameGroupNotFoundById_ReturnsNotFound()
	{
		_model.Id = "999";
		var result = await _model.OnGet();
		Assert.IsInstanceOfType(result, typeof(NotFoundResult));
	}

	[TestMethod]
	public async Task OnGet_GameGroupNotFoundByAbbreviation_ReturnsNotFound()
	{
		_model.Id = "NONEXISTENT";
		var result = await _model.OnGet();
		Assert.IsInstanceOfType(result, typeof(NotFoundResult));
	}

	[TestMethod]
	public async Task OnGet_ValidGameGroupById_LoadsGameGroupData()
	{
		var gameGroup = _db.AddGameGroup("Test Series", "TS").Entity;
		gameGroup.Description = "A test game series";
		await _db.SaveChangesAsync();
		_model.Id = gameGroup.Id.ToString();

		var result = await _model.OnGet();

		Assert.IsInstanceOfType(result, typeof(PageResult));
		Assert.AreEqual("Test Series", _model.Name);
		Assert.AreEqual("TS", _model.Abbreviation);
		Assert.AreEqual("A test game series", _model.Description);
		Assert.AreEqual(0, _model.Games.Count);
	}

	[TestMethod]
	public async Task OnGet_GameGroupWithGames_LoadsGameCount()
	{
		var gameGroup = _db.AddGameGroup("Test Series", "TS").Entity;
		var game1 = _db.AddGame("Test Game 1").Entity;
		game1.GameResourcesPage = "GameResources/TestGame1";
		var game2 = _db.AddGame("Test Game 2").Entity;

		_db.AttachToGroup(game1, gameGroup);
		_db.AttachToGroup(game2, gameGroup);

		await _db.SaveChangesAsync();

		_model.Id = gameGroup.Id.ToString();

		var result = await _model.OnGet();

		Assert.IsInstanceOfType(result, typeof(PageResult));
		Assert.AreEqual("Test Series", _model.Name);
		Assert.AreEqual(2, _model.Games.Count);

		var game1Entry = _model.Games.FirstOrDefault(g => g.Name == "Test Game 1");
		Assert.IsNotNull(game1Entry);
		Assert.AreEqual(game1.Id, game1Entry.Id);
		Assert.AreEqual("GameResources/TestGame1", game1Entry.GameResourcesPage);

		var game2Entry = _model.Games.FirstOrDefault(g => g.Name == "Test Game 2");
		Assert.IsNotNull(game2Entry);
		Assert.AreEqual(game2.Id, game2Entry.Id);
		Assert.IsNull(game2Entry.GameResourcesPage);
	}

	[TestMethod]
	public void AllowsAnonymousAttribute() => AssertAllowsAnonymousUsers(typeof(IndexModel));
}
