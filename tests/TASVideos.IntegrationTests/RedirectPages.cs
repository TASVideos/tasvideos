namespace TASVideos.IntegrationTests;

[TestClass]
#pragma warning disable CA1001
public class RedirectPagesTests
#pragma warning restore CA1001
{
	private TASVideosWebApplicationFactory _factory = null!;
	private HttpClient _client = null!;

	[TestInitialize]
	public void Setup()
	{
		_factory = new TASVideosWebApplicationFactory();
		_client = _factory.CreateClientWithNoFollowRedirects();
	}

	[TestCleanup]
	public void Cleanup()
	{
		_client.Dispose();
		_factory.Dispose();
	}

	[TestMethod]
	[DataRow("/Profile/Settings")]
	[DataRow("/AwardsEditor/2025")]
	[DataRow("/UserFiles/Upload")]
	public async Task AuthorizedPages_WithoutAuth_RedirectsToLogin(string path)
	{
		var response = await _client.GetAsync(path);

		Assert.AreEqual(HttpStatusCode.Redirect, response.StatusCode);
	}
}
