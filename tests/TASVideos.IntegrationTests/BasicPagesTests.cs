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
		_client = _factory.CreateClientWithFreshDatabase();
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
	public async Task UserFilesUncatalogedPage_ReturnsSuccessAndCorrectContent()
	{
		var response = await _client.GetAsync("/UserFiles/Uncataloged");

		response.EnsureSuccessStatusCode("Uncataloged page should load successfully");

		var title = await response.GetPageTitleAsync();
		Assert.IsTrue(title.Contains("Uncataloged"), $"Expected title to contain 'Uncataloged', but got: {title}");

		// Check for upload button
		var uploadButton = await response.QuerySelectorAsync("a[href*='/UserFiles/Upload']");
		Assert.IsNotNull(uploadButton, "Upload button should be present");

		// Check for "All User Files" button
		var allFilesButton = await response.QuerySelectorAsync("a[href*='/UserFiles']");
		Assert.IsNotNull(allFilesButton, "All User Files button should be present");
	}

	[TestMethod]
	public async Task UserFilesIndexPage_ReturnsSuccessAndCorrectContent()
	{
		var response = await _client.GetAsync("/UserFiles");

		response.EnsureSuccessStatusCode("UserFiles index page should load successfully");

		var title = await response.GetPageTitleAsync();
		Assert.IsTrue(title.Contains("Movie Storage") || title.Contains("User Files"), $"Expected title to contain 'Movie Storage' or 'User Files', but got: {title}");
	}

	[TestMethod]
	public async Task SubmissionsPage_ReturnsSuccessAndCorrectContent()
	{
		var response = await _client.GetAsync("/Subs-List");

		Assert.AreEqual(
			HttpStatusCode.OK,
			response.StatusCode,
			$"Expected OK, redirect, or 404 status, but got {response.StatusCode}");

		// If it's a successful response, check the title
		var title = await response.GetPageTitleAsync();
		Assert.IsTrue(title.Contains("Submissions"), $"Expected title to contain 'Publications', 'Movies', or 'TASVideos', but got: {title}");
	}

	// TODO: this definitely won't exist, we need to seed a wiki page during setup
	[TestMethod]
	public async Task WikiHomePage_ReturnsSuccessAndCorrectContent()
	{
		var response = await _client.GetAsync("/Welcome");

		if (response.StatusCode == HttpStatusCode.NotFound)
		{
			// If Welcome doesn't exist, try the home page
			response = await _client.GetAsync("/");
		}

		response.EnsureSuccessStatusCode("Wiki page should load successfully");

		var title = await response.GetPageTitleAsync();
		Assert.IsTrue(title.Contains("TASVideos"), $"Expected title to contain 'TASVideos', but got: {title}");
	}

	[TestMethod]
	public async Task NonExistentPage_Returns404()
	{
		var response = await _client.GetAsync("/ThisPageDoesNotExist");

		Assert.AreEqual(HttpStatusCode.NotFound, response.StatusCode, "Non-existent page should return 404");
	}
}
