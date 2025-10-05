using TASVideos.Pages.Publications;
using TASVideos.Tests.Base;

namespace TASVideos.RazorPages.Tests.Pages.Publications;

[TestClass]
public class LegacyMoviesModelTests : TestDbBase
{
	private readonly LegacyMoviesModel _page;

	public LegacyMoviesModelTests()
	{
		_page = new LegacyMoviesModel(_db);
	}

	[TestMethod]
	[DataRow(null)]
	[DataRow("")]
	[DataRow("   ")]
	public async Task OnGet_NoParameters_RedirectsToMovies(string id)
	{
		_page.Id = id;
		var result = await _page.OnGet();
		AssertRedirectToMovies(result);
	}

	[TestMethod]
	[DataRow("123", "123M")]
	[DataRow("123,456,789", "123M-456M-789M")]
	[DataRow("123,invalid,456,NotANumber,789", "123M-456M-789M")]
	public async Task OnGet_Ids_RedirectsToPublicationsWithQuery(string id, string expectedQuery)
	{
		_page.Id = id;
		var result = await _page.OnGet();
		AssertPubRedirect(result, expectedQuery);
	}

	[TestMethod]
	public async Task OnGet_GameNameByDisplayName_RedirectsToPublicationsWithGameToken()
	{
		var game = _db.AddGame("Super Mario Bros.").Entity;
		await _db.SaveChangesAsync();
		_page.Name = game.DisplayName;

		var result = await _page.OnGet();

		AssertPubRedirect(result, $"{game.Id}G");
	}

	[TestMethod]
	public async Task OnGet_GameNameByAbbreviation_RedirectsToPublicationsWithGameToken()
	{
		var game = _db.AddGame(null, "LOZ").Entity;
		await _db.SaveChangesAsync();
		_page.Name = game.Abbreviation;

		var result = await _page.OnGet();

		AssertPubRedirect(result, $"{game.Id}G");
	}

	[TestMethod]
	public async Task OnGet_GameNameNotFound_RedirectsToMovies()
	{
		_page.Name = "NonexistentGame";

		var result = await _page.OnGet();

		AssertRedirectToMovies(result);
	}

	[TestMethod]
	[DataRow("Y")]
	[DataRow("N")] // rec=N is treated same as rec=Y
	[DataRow("SomeValue")]
	public async Task OnGet_RecParameter_AddsNewcomerRecToken(string rec)
	{
		_page.Rec = rec;

		var result = await _page.OnGet();

		AssertPubRedirect(result, "NewcomerRec");
	}

	[TestMethod]
	[DataRow("")]
	[DataRow("    ")]
	public async Task OnGet_EmptyRecParameter_DoesNotAddToken(string rec)
	{
		_page.Rec = rec;
		var result = await _page.OnGet();
		AssertRedirectToMovies(result);
	}

	[TestMethod]
	public async Task OnGet_GameNameAndRec_CombinesTokens()
	{
		var game = _db.AddGame("Test Game").Entity;
		await _db.SaveChangesAsync();
		_page.Name = game.DisplayName;
		_page.Rec = "Y";

		var result = await _page.OnGet();

		AssertPubRedirect(result, $"{game.Id}G-NewcomerRec");
	}

	[TestMethod]
	public async Task OnGet_IdSupercedesOtherParameters_RedirectsWithIdOnly()
	{
		_db.AddGame("Some Game");
		await _db.SaveChangesAsync();

		_page.Id = "333,444";
		_page.Name = "Some Game";
		_page.Rec = "Y";

		var result = await _page.OnGet();

		AssertPubRedirect(result, "333M-444M");
	}

	[TestMethod]
	public async Task OnGet_NonexistentGameWithRec_OnlyRecTokenAdded()
	{
		_page.Name = "NonexistentGame";
		_page.Rec = "Y";

		var result = await _page.OnGet();

		AssertPubRedirect(result, "NewcomerRec");
	}

	private static void AssertPubRedirect(IActionResult result, string query)
	{
		AssertRedirect(result, "/Publications/Index");
		var redirect = (RedirectToPageResult)result;
		Assert.IsNotNull(redirect.RouteValues);
		Assert.AreEqual(query, redirect.RouteValues!["query"]);
	}

	private static void AssertRedirectToMovies(IActionResult result)
	{
		Assert.IsInstanceOfType<RedirectResult>(result);
		var redirect = (RedirectResult)result;
		Assert.AreEqual("Movies", redirect.Url);
	}
}
