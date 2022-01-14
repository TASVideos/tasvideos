using System.Threading.Tasks;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using TASVideos.Core.Services.Youtube;
using TASVideos.Core.Settings;
using TASVideos.Data.Entity;
using TASVideos.Tests.Base;

namespace TASVideos.Core.Tests.Services
{
	[TestClass]
	public class YoutubeSyncTests
	{
		private readonly YouTubeSync _youTubeSync;

		public YoutubeSyncTests()
		{
			var clientFactoryMock = HttpClientFactoryMock.Create();
			var mockAuth = new Mock<IGoogleAuthService>();
			_youTubeSync = new (clientFactoryMock.Object, mockAuth.Object, new TestWikiToTextRenderer(), new AppSettings(), NullLogger<YouTubeSync>.Instance);
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

		private class TestWikiToTextRenderer : IWikiToTextRenderer
		{
			public async Task<string> RenderWikiForYoutube(WikiPage page) => await Task.FromResult("");
		}
	}
}
