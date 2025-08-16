namespace TASVideos.IntegrationTests;

[TestClass]
#pragma warning disable CA1001
public class UserFilesIntegrationTests
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
	public async Task UncatalogedPage_WithNoFiles_ShowsEmptyState()
	{
		// No seed data so page should be empty
		var response = await _client.GetAsync("/UserFiles/Uncataloged");

		response.EnsureSuccessStatusCode("Uncataloged page should load successfully even with no data");

		var title = await response.GetPageTitleAsync();
		Assert.IsTrue(
			title.Contains("Uncataloged User Files (0)"),
			$"Title should indicate 0 files, but got: {title}");
	}

	[TestMethod]
	public async Task UserFilesIndex_LoadsSuccessfully()
	{
		var response = await _client.GetAsync("/UserFiles");

		response.EnsureSuccessStatusCode("UserFiles index should load successfully");

		// Check for key page elements
		var hasNavigation = await response.QuerySelectorAsync("nav");
		Assert.IsNotNull(hasNavigation, "Page should have navigation");
	}
}
