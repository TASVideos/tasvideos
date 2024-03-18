using System.Net;
using Microsoft.Extensions.Logging.Abstractions;
using TASVideos.Core.Services.Wiki;
using TASVideos.Core.Services.Youtube;
using TASVideos.Core.Settings;

namespace TASVideos.Core.Tests.Services;

[TestClass]
public class YoutubeSyncTests
{
	private readonly YouTubeSync _youTubeSync;

	private class MockHttpMessageHandler(string response, HttpStatusCode statusCode) : HttpMessageHandler
	{
		protected override async Task<HttpResponseMessage> SendAsync(
			HttpRequestMessage request,
			CancellationToken cancellationToken)
		{
			return await Task.FromResult(new HttpResponseMessage
			{
				StatusCode = statusCode,
				Content = new StringContent(response)
			});
		}
	}

	private static IHttpClientFactory HttpClientFactoryMockCreate()
	{
		var messageHandler = new MockHttpMessageHandler("TEST VALUE", HttpStatusCode.OK);
		var httpClient = new HttpClient(messageHandler);

		var factory = Substitute.For<IHttpClientFactory>();
		factory.CreateClient(HttpClients.Youtube).Returns(httpClient);
		return factory;
	}

	public YoutubeSyncTests()
	{
		var clientFactoryMock = HttpClientFactoryMockCreate();
		var mockAuth = Substitute.For<IGoogleAuthService>();
		_youTubeSync = new(clientFactoryMock, mockAuth, new TestWikiToTextRenderer(), new AppSettings(), NullLogger<YouTubeSync>.Instance);
	}

	[TestMethod]
	[DataRow("", "")]
	[DataRow("https://www.youtube.com/watch?v=12345", "12345")]
	[DataRow("v=12345", "12345")]
	[DataRow("https://www.youtube.com/watch?v=12345#ytd-watch", "12345")]
	[DataRow("https://www.youtube.com/watch?v=12345&list=ABCDE", "12345")]
	[DataRow("https://www.youtube.com/watch?list=ABCDE&v=12345", "12345")]
	[DataRow("https://www.youtube.com/watch?list=ABCDE&v=12345&index=2", "12345")]
	[DataRow("https://www.youtube.com/watch?index=2&fmt=37&v=12345", "12345")]
	[DataRow("v=12345?", "12345")]
	[DataRow("https://youtu.be/12345", "12345")]
	public void VideoId(string url, string expected)
	{
		var actual = _youTubeSync.VideoId(url);
		Assert.AreEqual(expected, actual);
	}

	[TestMethod]
	[DataRow("", false)]
	[DataRow("https://www.youtube.com/watch?v=12345", true)]
	[DataRow("https://youtube.com/watch?v=12345", true)]
	[DataRow("https://youtu.be/12345", true)]
	[DataRow("https://youtube.com/watch?v=12345&fmt=37", true)]
	[DataRow("https://youtube.com/watch?fmt=37&v=12345", true)]
	public void IsYoutubeUrl(string url, bool expected)
	{
		var actual = _youTubeSync.IsYoutubeUrl(url);
		Assert.AreEqual(expected, actual);
	}

	[TestMethod]
	[DataRow(null, null)]
	[DataRow("", null)]
	[DataRow("https://www.youtube.com/watch?v=12345", "https://www.youtube.com/embed/12345")]
	[DataRow("http://www.youtube.com/watch?v=12345", "https://www.youtube.com/embed/12345")]
	[DataRow("https://www.youtube.com/embed/12345", "https://www.youtube.com/embed/12345")]
	public void ConvertToEmbedLink(string? url, string? expected)
	{
		var actual = _youTubeSync.ConvertToEmbedLink(url);
		Assert.AreEqual(expected, actual);
	}

	private class TestWikiToTextRenderer : IWikiToTextRenderer
	{
		public async Task<string> RenderWikiForYoutube(IWikiPage page) => await Task.FromResult("");
	}
}
