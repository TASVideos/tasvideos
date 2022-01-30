using TASVideos.Core.Services.ExternalMediaPublisher;
using TASVideos.Core.Settings;

namespace TASVideos.Core.Tests.Services;

[TestClass]
public class ExternalMediaPublisherTests
{
	[TestMethod]
	[DataRow("https://localhost", "1111S", "https://localhost/1111S")]
	[DataRow("https://localhost/", "1111S", "https://localhost/1111S")]
	public void ToAbsolute(string baseUrl, string relativeLink, string expected)
	{
		var publisher = new ExternalMediaPublisher(
			new AppSettings { BaseUrl = baseUrl },
			Enumerable.Empty<IPostDistributor>());

		var actual = publisher.ToAbsolute(relativeLink);
		Assert.AreEqual(expected, actual);
	}
}
