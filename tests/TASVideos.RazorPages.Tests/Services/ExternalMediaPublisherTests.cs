using TASVideos.Core.Settings;
using TASVideos.Services;

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
			[],
			null!);

		var actual = publisher.ToAbsolute(relativeLink);
		Assert.AreEqual(expected, actual);
	}
}
