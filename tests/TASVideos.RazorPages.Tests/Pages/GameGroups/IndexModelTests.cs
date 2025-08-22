using Microsoft.AspNetCore.Authorization;
using TASVideos.Data.Entity.Game;
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
		var gameGroup = _db.GameGroups.Add(new GameGroup
		{
			Name = "Test Series",
			Abbreviation = "TS",
			Description = "A test game series"
		}).Entity;
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
		var gameGroup = _db.GameGroups.Add(new GameGroup
		{
			Name = "Test Series",
			Abbreviation = "TS"
		}).Entity;

		var game1 = _db.Games.Add(new Game
		{
			DisplayName = "Test Game 1",
			GameResourcesPage = "GameResources/TestGame1"
		}).Entity;

		var game2 = _db.Games.Add(new Game
		{
			DisplayName = "Test Game 2"
		}).Entity;

		_db.GameGameGroups.Add(new GameGameGroup { Game = game1, GameGroup = gameGroup });
		_db.GameGameGroups.Add(new GameGameGroup { Game = game2, GameGroup = gameGroup });

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
	public void IndexModel_HasAllowAnonymousAttribute()
	{
		var type = typeof(IndexModel);
		var attribute = type.GetCustomAttributes(typeof(AllowAnonymousAttribute), false).FirstOrDefault();

		Assert.IsNotNull(attribute);
	}
}
