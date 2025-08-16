namespace TASVideos.IntegrationTests;

[TestClass]
#pragma warning disable CA1001
public class BasicPagesTests
#pragma warning restore CA1001
{
	private TASVideosWebApplicationFactory _factory = null!;
	private HttpClient _client = null!;

	[TestInitialize]
	public void Setup()
	{
		_factory = new TASVideosWebApplicationFactory();
		_client = _factory.CreateClientWithFollowRedirects();
	}

	[TestCleanup]
	public void Cleanup()
	{
		_client.Dispose();
		_factory.Dispose();
	}

	[TestMethod]
	public async Task HomePage_ReturnsSuccessAndCorrectTitle()
	{
		var response = await _client.GetAsync("/");

		response.EnsureSuccessStatusCode("Home page should load successfully");

		var title = await response.GetPageTitleAsync();
		Assert.IsTrue(title.Contains("TASVideos"), $"Expected title to contain 'TASVideos', but got: {title}");
	}

	[TestMethod]
	public async Task NonExistentPage_Returns404()
	{
		var response = await _client.GetAsync("/ThisPageDoesNotExist");

		Assert.AreEqual(HttpStatusCode.NotFound, response.StatusCode);
	}

	[TestMethod]
	public async Task UserFilesUncatalogedPage_ReturnsSuccessAndCorrectContent()
	{
		var response = await _client.GetAsync("/UserFiles/Uncataloged");

		Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);

		var title = await response.GetPageTitleAsync();
		Assert.IsTrue(title.Contains("Uncataloged"), $"Expected title to contain 'Uncataloged', but got: {title}");

		var uploadButton = await response.QuerySelectorAsync("a[href*='/UserFiles/Upload']");
		Assert.IsNotNull(uploadButton);

		var allFilesButton = await response.QuerySelectorAsync("a[href*='/UserFiles']");
		Assert.IsNotNull(allFilesButton);
	}

	[TestMethod]
	public async Task UserFilesIndexPage_ReturnsSuccessAndCorrectContent()
	{
		var response = await _client.GetAsync("/UserFiles");

		Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);

		var title = await response.GetPageTitleAsync();
		Assert.IsTrue(title.Contains("Movie Storage"), $"Expected title to contain 'Movie Storage' but got: {title}");
	}

	[TestMethod]
	public async Task SubmissionsPage_ReturnsSuccessAndCorrectContent()
	{
		var response = await _client.GetAsync("/Subs-List");

		Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);

		var title = await response.GetPageTitleAsync();
		Assert.IsTrue(title.Contains("Submissions"), $"Expected title to contain 'Submissions', but got: {title}");
	}
}
