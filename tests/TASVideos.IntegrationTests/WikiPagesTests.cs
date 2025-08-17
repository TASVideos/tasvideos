using TASVideos.Data.Entity;

namespace TASVideos.IntegrationTests;

[TestClass]
#pragma warning disable CA1001
public class WikiPagesTests
#pragma warning restore CA1001
{
	private const string PageName = "Welcome";
	private const string Markup = "Welcome to TASVideos";

	private TASVideosWebApplicationFactory _factory = null!;
	private HttpClient _client = null!;

	[TestInitialize]
	public void Setup()
	{
		_factory = new TASVideosWebApplicationFactory(true);
		_client = _factory.CreateClientWithFollowRedirects();

		_factory.SeedDatabase(context =>
		{
			var author = context.Users.Add(new User
			{
				UserName = "TestUser",
				NormalizedUserName = "TESTUSER",
				Email = "test@example.com",
				NormalizedEmail = "TEST@EXAMPLE.COM",
				CreateTimestamp = DateTime.UtcNow,
				EmailConfirmed = true,
				SecurityStamp = Guid.NewGuid().ToString(),
				ConcurrencyStamp = Guid.NewGuid().ToString()
			}).Entity;

			context.WikiPages.Add(new WikiPage
			{
				PageName = PageName,
				Markup = Markup,
				Revision = 1,
				Author = author,
				CreateTimestamp = DateTime.UtcNow,
				LastUpdateTimestamp = DateTime.UtcNow
			});

			context.WikiReferrals.Add(new WikiPageReferral
			{
				Excerpt = "link",
				Referral = PageName,
				Referrer = "SomeOtherPage"
			});
		});
	}

	[TestCleanup]
	public void Cleanup()
	{
		_client.Dispose();
		_factory.Dispose();
	}

	[TestMethod]
	public async Task WikiPage()
	{
		var response = await _client.GetAsync("/Welcome");

		Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);

		var rendersMarkup = await response.ContainsTextAsync(Markup);
		Assert.IsTrue(rendersMarkup, "Expected title to contain markup");
	}

	[TestMethod]
	public async Task PageHistory()
	{
		var response = await _client.GetAsync($"/Wiki/PageHistory?path={PageName}");

		Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);

		var revisionLink = await response.QuerySelectorAsync($"a[href*='/{PageName}?revision=1']");
		Assert.IsNotNull(revisionLink);
	}

	[TestMethod]
	public async Task LatestDiff()
	{
		var response = await _client.GetAsync($"/Wiki/PageHistory?path={PageName}&latest=true");

		Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);

		var revisionLink = await response.QuerySelectorAsync($"a[href*='/{PageName}?revision=1']");
		Assert.IsNotNull(revisionLink);

		var diff = await response.QuerySelectorAsync("div[id*=diff-view]");
		Assert.IsNotNull(diff);
	}

	[TestMethod]
	public async Task Referrers()
	{
		var response = await _client.GetAsync($"/Wiki/Referrers?path={PageName}");

		Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);

		var referrerCards = await response.QuerySelectorAllAsync("div.card");
		Assert.AreEqual(1, referrerCards.Count());
	}

	[TestMethod]
	public async Task ViewSource()
	{
		var response = await _client.GetAsync($"/Wiki/ViewSource?path={PageName}");

		Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);

		var srcContainer = await response.QuerySelectorAsync("pre");
		Assert.IsNotNull(srcContainer);
	}
}
