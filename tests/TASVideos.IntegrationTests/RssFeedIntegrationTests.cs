using System.Xml.Linq;

namespace TASVideos.IntegrationTests;

[TestClass]
#pragma warning disable CA1001
public class RssFeedIntegrationTests
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
	[DataRow("/publications.rss", "Publications RSS Feed")]
	[DataRow("/submissions.rss", "Submissions RSS Feed")]
	[DataRow("/wiki.rss", "Wiki RSS Feed")]
	[DataRow("/news.rss", "News RSS Feed")]
	public async Task RssFeed_ReturnsSuccessAndValidGenerator(string url, string feedDescription)
	{
		var response = await _client.GetAsync(url);

		Assert.AreEqual(HttpStatusCode.OK, response.StatusCode, $"{feedDescription} should return 200 OK status code");
		Assert.IsNotNull(response.Content.Headers.ContentType);
		Assert.IsNotNull(response.Content.Headers.ContentType.MediaType);
		Assert.IsTrue(response.Content.Headers.ContentType.MediaType.Contains("xml"), $"{feedDescription} should return XML content type");

		var rssContent = await response.Content.ReadAsStringAsync();
		Assert.IsFalse(string.IsNullOrWhiteSpace(rssContent), $"{feedDescription} should return non-empty content");

		XDocument rssDoc;
		try
		{
			rssDoc = XDocument.Parse(rssContent);
		}
		catch (Exception ex)
		{
			Assert.Fail($"{feedDescription} should return valid XML. Error: {ex.Message}");
			return;
		}

		var rssElement = rssDoc.Root;
		Assert.IsNotNull(rssElement, $"{feedDescription} should have a root element");
		Assert.AreEqual("rss", rssElement.Name.LocalName, $"{feedDescription} root element should be 'rss'");

		var channelElement = rssElement.Element("channel");
		Assert.IsNotNull(channelElement, $"{feedDescription} should have a channel element");

		// Test that the generator tag exists and has the expected value
		var generatorElement = channelElement.Element("generator");
		Assert.IsNotNull(generatorElement, $"{feedDescription} should have a generator element");

		var generatorValue = generatorElement.Value;
		Assert.IsFalse(string.IsNullOrWhiteSpace(generatorValue), $"{feedDescription} generator should not be empty");

		Assert.IsTrue(generatorValue.Contains("TASVideos"), $"{feedDescription} generator should contain 'TASVideos', but got: {generatorValue}");

		var titleElement = channelElement.Element("title");
		Assert.IsNotNull(titleElement, $"{feedDescription} should have a title element");
		Assert.IsFalse(string.IsNullOrWhiteSpace(titleElement.Value), $"{feedDescription} title should not be empty");

		var linkElement = channelElement.Element("link");
		Assert.IsNotNull(linkElement, $"{feedDescription} should have a link element");
		Assert.IsFalse(string.IsNullOrWhiteSpace(linkElement.Value), $"{feedDescription} link should not be empty");

		var descriptionElement = channelElement.Element("description");
		Assert.IsNotNull(descriptionElement, $"{feedDescription} should have a description element");
		Assert.IsFalse(string.IsNullOrWhiteSpace(descriptionElement.Value), $"{feedDescription} description should not be empty");
	}
}
