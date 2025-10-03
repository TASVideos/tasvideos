using TASVideos.Core.Services;
using TASVideos.Data.Entity.Game;
using TASVideos.Pages.Publications;
using TASVideos.Tests.Base;

namespace TASVideos.RazorPages.Tests.Pages.Publications;

[TestClass]
public class FilterModelTests : TestDbBase
{
	private readonly IMovieSearchTokens _movieTokens;
	private readonly ITagService _tagService;
	private readonly IFlagService _flagService;
	private readonly FilterModel _page;

	public FilterModelTests()
	{
		_movieTokens = Substitute.For<IMovieSearchTokens>();
		_tagService = Substitute.For<ITagService>();
		_flagService = Substitute.For<IFlagService>();
		_page = new FilterModel(_db, _movieTokens, _tagService, _flagService);
	}

	[TestMethod]
	public async Task OnGet_WithQuery_ParsesTokensIntoSearch()
	{
		const string systemCode = "nes";
		const string pubClass = "standard";
		const string tag = "1p";
		const string flag = "atlas";
		var tokens = Substitute.For<IPublicationTokens>();
		tokens.SystemCodes.Returns([systemCode]);
		tokens.Classes.Returns([pubClass]);
		tokens.Tags.Returns([tag]);
		tokens.Flags.Returns([flag]);
		_movieTokens.GetTokens().Returns(tokens);
		_tagService.GetAll().Returns([new Tag { DisplayName = tag }]);
		_flagService.GetAll().Returns([new Flag { Name = flag }]);
		_page.Query = "NES-Standard-1p-atlas";

		await _page.OnGet();

		Assert.AreEqual(1, _page.Search.SystemCodes.Count);
		Assert.IsTrue(_page.Search.SystemCodes.Any(s => s == systemCode));
		Assert.AreEqual(1, _page.Search.Classes.Count);
		Assert.IsTrue(_page.Search.Classes.Any(c => c == pubClass));
		Assert.AreEqual(1, _page.Search.Tags.Count);
		Assert.IsTrue(_page.Search.Tags.Any(t => t == tag));
		Assert.AreEqual(1, _page.Search.Flags.Count);
		Assert.IsTrue(_page.Search.Flags.Any(f => f == flag));
	}

	[TestMethod]
	public async Task OnGet_PopulatesAvailableTags()
	{
		var tokens = Substitute.For<IPublicationTokens>();
		tokens.SystemCodes.Returns([]);
		tokens.Classes.Returns([]);
		tokens.Tags.Returns([]);
		tokens.Flags.Returns([]);

		var tags = new List<Tag>
		{
			new() { Id = 1, Code = "1p", DisplayName = "Single Player" },
			new() { Id = 2, Code = "2p", DisplayName = "Two Players" }
		};

		_movieTokens.GetTokens().Returns(tokens);
		_tagService.GetAll().Returns(tags);
		_flagService.GetAll().Returns([]);

		await _page.OnGet();

		Assert.AreEqual(2, _page.AvailableTags.Count);
		Assert.IsTrue(_page.AvailableTags.Any(t => t.Text == "Single Player"));
		Assert.IsTrue(_page.AvailableTags.Any(t => t.Text == "Two Players"));
	}

	[TestMethod]
	public async Task OnGet_PopulatesAvailableFlags()
	{
		var tokens = Substitute.For<IPublicationTokens>();
		tokens.SystemCodes.Returns([]);
		tokens.Classes.Returns([]);
		tokens.Tags.Returns([]);
		tokens.Flags.Returns([]);

		var flags = new List<Flag>
		{
			new() { Id = 1, Token = "atlas", Name = "Atlas" },
			new() { Id = 2, Token = "verified", Name = "Verified" }
		};

		_movieTokens.GetTokens().Returns(tokens);
		_tagService.GetAll().Returns([]);
		_flagService.GetAll().Returns(flags);

		await _page.OnGet();

		Assert.AreEqual(2, _page.AvailableFlags.Count);
		Assert.IsTrue(_page.AvailableFlags.Any(f => f.Text == "Atlas"));
		Assert.IsTrue(_page.AvailableFlags.Any(f => f.Text == "Verified"));
	}

	[TestMethod]
	public async Task OnGet_PopulatesGameGroups()
	{
		var tokens = Substitute.For<IPublicationTokens>();
		tokens.SystemCodes.Returns([]);
		tokens.Classes.Returns([]);
		tokens.Tags.Returns([]);
		tokens.Flags.Returns([]);

		_db.GameGroups.Add(new GameGroup
		{
			Id = 1,
			Name = "Super Mario"
		});
		await _db.SaveChangesAsync();

		_movieTokens.GetTokens().Returns(tokens);
		_tagService.GetAll().Returns([]);
		_flagService.GetAll().Returns([]);

		await _page.OnGet();

		Assert.AreEqual(1, _page.AvailableGameGroups.Count);
		Assert.AreEqual("Super Mario", _page.AvailableGameGroups.First().Text);
	}

	[TestMethod]
	public async Task OnGet_PopulatesPublishedAuthors()
	{
		var tokens = Substitute.For<IPublicationTokens>();
		tokens.SystemCodes.Returns([]);
		tokens.Classes.Returns([]);
		tokens.Tags.Returns([]);
		tokens.Flags.Returns([]);

		_db.AddPublication("PublishedAuthor");

		_db.AddUser("UnpublishedUser");
		await _db.SaveChangesAsync();

		_movieTokens.GetTokens().Returns(tokens);
		_tagService.GetAll().Returns([]);
		_flagService.GetAll().Returns([]);

		await _page.OnGet();

		Assert.IsTrue(_page.AvailableAuthors.Count >= 1);
		Assert.IsTrue(_page.AvailableAuthors.Any(a => a.Text == "PublishedAuthor"));
	}

	[TestMethod]
	public void OnPost_RedirectsToMoviesPageWithSearchUrl()
	{
		_page.Search = new IndexModel.PublicationSearch
		{
			SystemCodes = ["NES"],
			Classes = ["Standard"],
			Years = []
		};

		var actual = _page.OnPost();

		Assert.IsInstanceOfType<RedirectResult>(actual);
		var redirectResult = (RedirectResult)actual;
		Assert.IsTrue(redirectResult.Url.Contains("/Movies-"));
		Assert.IsTrue(redirectResult.Url.Contains("NES"));
		Assert.IsTrue(redirectResult.Url.Contains("Standard"));
	}

	[TestMethod]
	public void OnPost_EmptySearch_RedirectsToBasicMoviesPage()
	{
		_page.Search = new() { Years = [] };

		var actual = _page.OnPost();

		Assert.IsInstanceOfType<RedirectResult>(actual);
		var redirectResult = (RedirectResult)actual;
		Assert.AreEqual("/Movies-", redirectResult.Url);
	}
}
