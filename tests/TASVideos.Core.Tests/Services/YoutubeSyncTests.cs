using Microsoft.VisualStudio.TestTools.UnitTesting;
using TASVideos.Core.Services.Youtube;

namespace TASVideos.Core.Tests.Services
{
	[TestClass]
	public class YoutubeSyncTests
	{
		[TestMethod]
		[DataRow("https://www.youtube.com/watch?v=12345", "12345")]
		[DataRow("v=12345", "12345")]
		[DataRow("https://www.youtube.com/watch?v=12345#ytd-watch", "12345")]
		[DataRow("https://www.youtube.com/watch?v=12345&list=ABCDE", "12345")]
		[DataRow("https://www.youtube.com/watch?list=ABCDE&v=12345", "12345")]
		[DataRow("https://www.youtube.com/watch?list=ABCDE&v=12345&index=2", "12345")]
		[DataRow("v=12345?", "12345")]
		public void VideoId(string url, string expected)
		{
			var actual = YouTubeSync.VideoId(url);
			Assert.AreEqual(expected, actual);
		}
	}
}
