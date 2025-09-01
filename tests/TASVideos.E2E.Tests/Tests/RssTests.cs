using System.Xml;
using TASVideos.E2E.Tests.Base;

namespace TASVideos.E2E.Tests.Tests;

[TestClass]
public class RssTests : BaseE2ETest
{
	[TestMethod]
	[DataRow("publications")]
	[DataRow("submissions")]
	[DataRow("wiki")]
	[DataRow("news")]
	public async Task RssFeeds(string feed)
	{
		AssertEnabled();

		var response = await Navigate($"/{feed}.rss");

		AssertResponseCode(response, 200);

		var contentType = response!.Headers["content-type"];
		Assert.IsTrue(
			contentType.Contains("xml", StringComparison.OrdinalIgnoreCase),
			$"Content type should be XML but was: {contentType}");

		var responseBody = await response.TextAsync();
		Assert.IsFalse(string.IsNullOrWhiteSpace(responseBody), "Response body should not be empty");

		try
		{
			var xmlDocument = new XmlDocument();
			xmlDocument.LoadXml(responseBody);
			Assert.IsNotNull(xmlDocument.DocumentElement, "XML document should have a root element");
		}
		catch (XmlException ex)
		{
			Assert.Fail($"XML parsing failed: {ex.Message}");
		}
	}
}
