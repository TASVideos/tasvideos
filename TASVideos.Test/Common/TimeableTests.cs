using Microsoft.VisualStudio.TestTools.UnitTesting;
using TASVideos.Common;

namespace TASVideos.Test.Common
{
	[TestClass]
	public class TimeableTests
	{
		[TestMethod]
		[DataRow(60.1, 421, "0:00:07")]
		[DataRow(75, 2346, "0:00:31.28")]
		[DataRow(60, 412521, "1:54:35.35")]
		[DataRow(58.643, 5213, "0:01:28.89")]
		[DataRow(62.5, 11221, "0:02:59.54")]
		[DataRow(50, 6666, "0:02:13.32")]
		[DataRow(60.09, 325523, "1:30:17.26")]
		public void TimeableTimespanTest(double frameRate, int frames, string expected)
		{
			var actual = new Timeable { FrameRate = frameRate, Frames = frames }
				.Time()
				.ToString("g");
			Assert.AreEqual(expected, actual);
		}
	}
}
