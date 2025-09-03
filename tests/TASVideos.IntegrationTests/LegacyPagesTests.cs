namespace TASVideos.IntegrationTests;

[TestClass]
#pragma warning disable CA1001
public class LegacyPagesTests
#pragma warning restore CA1001
{
	private TASVideosWebApplicationFactory _factory = null!;
	private HttpClient _client = null!;

	[TestInitialize]
	public void Setup()
	{
		_factory = new TASVideosWebApplicationFactory(true);
		_client = _factory.CreateClientWithNoFollowRedirects();
	}

	[TestCleanup]
	public void Cleanup()
	{
		_client.Dispose();
		_factory.Dispose();
	}

	[TestMethod]
	[DataRow("privileges", "permissions")]
	[DataRow("forum/moodreport.php", "Forum/MoodReport")]
	[DataRow("forum/viewforum.php?f=123", "Forum/Subforum/123")]
	[DataRow("forum/f/123", "Forum/Subforum/123")]
	[DataRow("forum/t/456", "Forum/Topics/456")]
	[DataRow("forum/viewtopic.php?t=456", "Forum/Topics/456")]
	[DataRow("submitmovie", "Submissions/Submit")]
	[DataRow("queue.cgi", "Subs-List")]
	[DataRow("queue.cgi?mode=list", "Subs-List")]
	[DataRow("queue.cgi?mode=submit", "Submissions/Submit")]
	[DataRow("queue.cgi?mode=edit", "Subs-List")]
	[DataRow("queue.cgi?mode=edit&id=789", "Submissions/Edit/789")]
	[DataRow("queue.cgi?mode=view&id=789", "789S")]
	[DataRow("queue.cgi?id=789", "789S")]
	public async Task Basic_Redirects(string route, string expectedRedirect)
	{
		var response = await _client.GetAsync($"/{route}");
		Assert.AreEqual(HttpStatusCode.Found, response.StatusCode);
		Assert.IsNotNull(response.Headers.Location);
		Assert.AreEqual($"/{expectedRedirect.ToLower()}", response.Headers.Location.ToString().ToLower());
	}
}
