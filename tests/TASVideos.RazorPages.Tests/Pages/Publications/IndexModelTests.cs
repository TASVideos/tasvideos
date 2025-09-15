using TASVideos.Core.Services;
using TASVideos.Pages.Publications;
using TASVideos.Tests.Base;

namespace TASVideos.RazorPages.Tests.Pages.Publications;

[TestClass]
public class IndexModelTests : TestDbBase
{
	private readonly IMovieSearchTokens _movieTokens;
	private readonly IndexModel _page;

	public IndexModelTests()
	{
		_movieTokens = Substitute.For<IMovieSearchTokens>();
		_page = new IndexModel(_db, _movieTokens);
	}

	[TestMethod]
	public async Task OnGet_EmptyQuery_RedirectsToMovies()
	{
		var emptyTokens = CreateEmptyTokens();
		_movieTokens.GetTokens().Returns(emptyTokens);
		_page.Query = "";

		var result = await _page.OnGet();

		Assert.IsInstanceOfType<RedirectResult>(result);
		var redirectResult = (RedirectResult)result;
		Assert.AreEqual("Movies", redirectResult.Url);
	}

	[TestMethod]
	public async Task OnGet_EmptySearchModel_RedirectsToMovies()
	{
		var emptyTokens = CreateEmptyTokens();
		_movieTokens.GetTokens().Returns(emptyTokens);
		_page.Query = "invalid-token";

		var result = await _page.OnGet();

		Assert.IsInstanceOfType<RedirectResult>(result);
		var redirectResult = (RedirectResult)result;
		Assert.AreEqual("Movies", redirectResult.Url);
	}

	[TestMethod]
	public async Task OnGet_ValidQuery_ReturnsPage()
	{
		var tokens = CreateTokensWithClasses();
		_movieTokens.GetTokens().Returns(tokens);
		_page.Query = "standard";

		var result = await _page.OnGet();

		Assert.IsInstanceOfType<PageResult>(result);
	}

	private static IPublicationTokens CreateEmptyTokens()
	{
		var tokens = Substitute.For<IPublicationTokens>();
		tokens.SystemCodes.Returns([]);
		tokens.Classes.Returns([]);
		tokens.Years.Returns([]);
		tokens.Tags.Returns([]);
		tokens.Genres.Returns([]);
		tokens.Flags.Returns([]);
		tokens.Authors.Returns([]);
		tokens.MovieIds.Returns([]);
		tokens.Games.Returns([]);
		tokens.GameGroups.Returns([]);
		return tokens;
	}

	private static IPublicationTokens CreateTokensWithClasses()
	{
		var tokens = Substitute.For<IPublicationTokens>();
		tokens.SystemCodes.Returns([]);
		tokens.Classes.Returns(["standard", "moons"]);
		tokens.Years.Returns([]);
		tokens.Tags.Returns([]);
		tokens.Genres.Returns([]);
		tokens.Flags.Returns([]);
		tokens.Authors.Returns([]);
		tokens.MovieIds.Returns([]);
		tokens.Games.Returns([]);
		tokens.GameGroups.Returns([]);
		return tokens;
	}
}
