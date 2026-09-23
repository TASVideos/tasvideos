using TASVideos.Data.Entity;
using TASVideos.Data.Entity.Forum;

namespace TASVideos.IntegrationTests;

[TestClass]
#pragma warning disable CA1001
public class CacheHeaderTests
#pragma warning restore CA1001
{
	private TASVideosWebApplicationFactory _factory = null!;
	private HttpClient _client = null!;

	[TestInitialize]
	public void Setup()
	{
		_factory = new TASVideosWebApplicationFactory(true);
		_client = _factory.CreateClientWithFollowRedirects();

		_factory.SeedDatabase(db =>
		{
			var user = db.Users.Add(new User
			{
				UserName = "TestUser",
				NormalizedUserName = "TESTUSER",
				Email = "test@example.com",
				NormalizedEmail = "TEST@EXAMPLE.COM",
			}).Entity;

			db.WikiPages.Add(new WikiPage
			{
				PageName = "TestPage",
				Markup = "Test Page here.",
			});

			var forum = db.Forums.Add(new() { Category = new() }).Entity;
			db.ForumPosts.Add(new ForumPost
			{
				Text = "Test Post",
				Poster = user,
				Forum = forum,
				Topic = new()
				{
					Id = 1,
					Poster = user,
					Forum = forum
				}
			});

			db.SaveChanges();
		});
	}

	[TestCleanup]
	public void Cleanup()
	{
		_client.Dispose();
		_factory.Dispose();
	}

	[TestMethod]
	public async Task WikiPage_NotLoggedIn_NoCacheControl()
	{
		var response = await _client.GetAsync("/TestPage");
		response.EnsureSuccessStatusCode();
		Assert.IsNull(response.Headers.CacheControl);
	}

	[TestMethod]
	public async Task ForumPost_NotLoggedIn_NoCacheControl()
	{
		var response = await _client.GetAsync("/Forum/Topics/1");
		response.EnsureSuccessStatusCode();
		Assert.IsNull(response.Headers.CacheControl);
	}

	[TestMethod]
	public async Task LoginPage_HasCacheControl()
	{
		var response = await _client.GetAsync("/Account/Login");
		response.EnsureSuccessStatusCode();
		Assert.IsNotNull(response.Headers.CacheControl);
	}
}
