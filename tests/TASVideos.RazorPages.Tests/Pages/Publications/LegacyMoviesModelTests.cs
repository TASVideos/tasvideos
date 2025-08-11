using TASVideos.Data.Entity.Game;
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
	public async Task OnGet_NoParameters_RedirectsToMovies()
	{
		_page.Id = null;
		_page.Name = null;
		_page.Rec = null;

		var result = await _page.OnGet();

		Assert.IsInstanceOfType<RedirectResult>(result);
		var redirect = (RedirectResult)result;
		Assert.AreEqual("Movies", redirect.Url);
	}

	[TestMethod]
	public async Task OnGet_SingleId_RedirectsToPublicationsWithQuery()
	{
		_page.Id = "123";

		var result = await _page.OnGet();

		Assert.IsInstanceOfType<RedirectToPageResult>(result);
		var redirect = (RedirectToPageResult)result;
		Assert.AreEqual("/Publications/Index", redirect.PageName);
		Assert.AreEqual("123M", redirect.RouteValues!["query"]);
	}

	[TestMethod]
	public async Task OnGet_MultipleIds_RedirectsToPublicationsWithHyphenSeparatedQuery()
	{
		_page.Id = "123,456,789";

		var result = await _page.OnGet();

		Assert.IsInstanceOfType<RedirectToPageResult>(result);
		var redirect = (RedirectToPageResult)result;
		Assert.AreEqual("/Publications/Index", redirect.PageName);
		Assert.AreEqual("123M-456M-789M", redirect.RouteValues!["query"]);
	}

	[TestMethod]
	public async Task OnGet_IdWithInvalidValues_FiltersOutNonIntegers()
	{
		_page.Id = "123,invalid,456,notanumber,789";

		var result = await _page.OnGet();

		Assert.IsInstanceOfType<RedirectToPageResult>(result);
		var redirect = (RedirectToPageResult)result;
		Assert.AreEqual("/Publications/Index", redirect.PageName);
		Assert.AreEqual("123M-456M-789M", redirect.RouteValues!["query"]);
	}

	[TestMethod]
	public async Task OnGet_EmptyId_TreatedAsNoId()
	{
		_page.Id = "";
		_page.Name = null;
		_page.Rec = null;

		var result = await _page.OnGet();

		Assert.IsInstanceOfType<RedirectResult>(result);
		var redirect = (RedirectResult)result;
		Assert.AreEqual("Movies", redirect.Url);
	}

	[TestMethod]
	public async Task OnGet_WhitespaceOnlyId_TreatedAsNoId()
	{
		_page.Id = "   ";
		_page.Name = null;
		_page.Rec = null;

		var result = await _page.OnGet();

		Assert.IsInstanceOfType<RedirectResult>(result);
		var redirect = (RedirectResult)result;
		Assert.AreEqual("Movies", redirect.Url);
	}

	[TestMethod]
	public async Task OnGet_GameNameByDisplayName_RedirectsToPublicationsWithGameToken()
	{
		_db.Games.Add(new Game { Id = 42, DisplayName = "Super Mario Bros.", Abbreviation = "SMB" });
		await _db.SaveChangesAsync();
		_page.Name = "Super Mario Bros.";

		var result = await _page.OnGet();

		Assert.IsInstanceOfType<RedirectToPageResult>(result);
		var redirect = (RedirectToPageResult)result;
		Assert.AreEqual("/Publications/Index", redirect.PageName);
		Assert.AreEqual("42G", redirect.RouteValues!["query"]);
	}

	[TestMethod]
	public async Task OnGet_GameNameByAbbreviation_RedirectsToPublicationsWithGameToken()
	{
		_db.Games.Add(new Game { Id = 55, DisplayName = "The Legend of Zelda", Abbreviation = "LOZ" });
		await _db.SaveChangesAsync();
		_page.Name = "LOZ";

		var result = await _page.OnGet();

		Assert.IsInstanceOfType<RedirectToPageResult>(result);
		var redirect = (RedirectToPageResult)result;
		Assert.AreEqual("/Publications/Index", redirect.PageName);
		Assert.AreEqual("55G", redirect.RouteValues!["query"]);
	}

	[TestMethod]
	public async Task OnGet_GameNameNotFound_RedirectsToMovies()
	{
		_page.Name = "NonexistentGame";

		var result = await _page.OnGet();

		Assert.IsInstanceOfType<RedirectResult>(result);
		var redirect = (RedirectResult)result;
		Assert.AreEqual("Movies", redirect.Url);
	}

	[TestMethod]
	public async Task OnGet_EmptyGameName_RedirectsToMovies()
	{
		_page.Name = "";

		var result = await _page.OnGet();

		Assert.IsInstanceOfType<RedirectResult>(result);
		var redirect = (RedirectResult)result;
		Assert.AreEqual("Movies", redirect.Url);
	}

	[TestMethod]
	public async Task OnGet_WhitespaceOnlyGameName_RedirectsToMovies()
	{
		_page.Name = "   ";

		var result = await _page.OnGet();

		Assert.IsInstanceOfType<RedirectResult>(result);
		var redirect = (RedirectResult)result;
		Assert.AreEqual("Movies", redirect.Url);
	}

	[TestMethod]
	public async Task OnGet_RecParameterY_AddsNewcomerRecToken()
	{
		_page.Rec = "Y";

		var result = await _page.OnGet();

		Assert.IsInstanceOfType<RedirectToPageResult>(result);
		var redirect = (RedirectToPageResult)result;
		Assert.AreEqual("/Publications/Index", redirect.PageName);
		Assert.AreEqual("NewcomerRec", redirect.RouteValues!["query"]);
	}

	[TestMethod]
	public async Task OnGet_RecParameterN_AddsNewcomerRecToken()
	{
		_page.Rec = "N"; // rec=N is treated same as rec=Y

		var result = await _page.OnGet();

		Assert.IsInstanceOfType<RedirectToPageResult>(result);
		var redirect = (RedirectToPageResult)result;
		Assert.AreEqual("/Publications/Index", redirect.PageName);
		Assert.AreEqual("NewcomerRec", redirect.RouteValues!["query"]);
	}

	[TestMethod]
	public async Task OnGet_RecParameterAnyValue_AddsNewcomerRecToken()
	{
		_page.Rec = "SomeValue";

		var result = await _page.OnGet();

		Assert.IsInstanceOfType<RedirectToPageResult>(result);
		var redirect = (RedirectToPageResult)result;
		Assert.AreEqual("/Publications/Index", redirect.PageName);
		Assert.AreEqual("NewcomerRec", redirect.RouteValues!["query"]);
	}

	[TestMethod]
	public async Task OnGet_EmptyRecParameter_DoesNotAddToken()
	{
		_page.Rec = "";

		var result = await _page.OnGet();

		Assert.IsInstanceOfType<RedirectResult>(result);
		var redirect = (RedirectResult)result;
		Assert.AreEqual("Movies", redirect.Url);
	}

	[TestMethod]
	public async Task OnGet_WhitespaceOnlyRecParameter_DoesNotAddToken()
	{
		_page.Rec = "   ";

		var result = await _page.OnGet();

		Assert.IsInstanceOfType<RedirectResult>(result);
		var redirect = (RedirectResult)result;
		Assert.AreEqual("Movies", redirect.Url);
	}

	[TestMethod]
	public async Task OnGet_GameNameAndRec_CombinesTokens()
	{
		var game = new Game { Id = 99, DisplayName = "Test Game", Abbreviation = "TG" };
		_db.Games.Add(game);
		await _db.SaveChangesAsync();
		_page.Name = "Test Game";
		_page.Rec = "Y";

		var result = await _page.OnGet();

		Assert.IsInstanceOfType<RedirectToPageResult>(result);
		var redirect = (RedirectToPageResult)result;
		Assert.AreEqual("/Publications/Index", redirect.PageName);
		Assert.AreEqual("99G-NewcomerRec", redirect.RouteValues!["query"]);
	}

	[TestMethod]
	public async Task OnGet_IdSupercedesOtherParameters_RedirectsWithIdOnly()
	{
		var game = new Game { Id = 100, DisplayName = "Some Game", Abbreviation = "SG" };
		_db.Games.Add(game);
		await _db.SaveChangesAsync();

		_page.Id = "333,444";
		_page.Name = "Some Game";
		_page.Rec = "Y";

		var result = await _page.OnGet();

		Assert.IsInstanceOfType<RedirectToPageResult>(result);
		var redirect = (RedirectToPageResult)result;
		Assert.AreEqual("/Publications/Index", redirect.PageName);
		Assert.AreEqual("333M-444M", redirect.RouteValues!["query"]);
	}

	[TestMethod]
	public async Task OnGet_NonexistentGameWithRec_OnlyRecTokenAdded()
	{
		_page.Name = "NonexistentGame";
		_page.Rec = "Y";

		var result = await _page.OnGet();

		Assert.IsInstanceOfType<RedirectToPageResult>(result);
		var redirect = (RedirectToPageResult)result;
		Assert.AreEqual("/Publications/Index", redirect.PageName);
		Assert.AreEqual("NewcomerRec", redirect.RouteValues!["query"]);
	}

	[TestMethod]
	public async Task OnGet_NegativeIds_HandledCorrectly()
	{
		_page.Id = "-1,5,-10";

		var result = await _page.OnGet();

		Assert.IsInstanceOfType<RedirectToPageResult>(result);
		var redirect = (RedirectToPageResult)result;
		Assert.AreEqual("/Publications/Index", redirect.PageName);
		Assert.AreEqual("-1M-5M--10M", redirect.RouteValues!["query"]);
	}

	[TestMethod]
	public async Task OnGet_ZeroId_HandledCorrectly()
	{
		_page.Id = "0";

		var result = await _page.OnGet();

		Assert.IsInstanceOfType<RedirectToPageResult>(result);
		var redirect = (RedirectToPageResult)result;
		Assert.AreEqual("/Publications/Index", redirect.PageName);
		Assert.AreEqual("0M", redirect.RouteValues!["query"]);
	}
}
