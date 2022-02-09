namespace TASVideos.Common.Tests;

[TestClass]
public class TimeableTests
{
	[TestMethod]
	[DataRow(60.1, 421, "00:07.00")]
	[DataRow(75, 2346, "00:31.28")]
	[DataRow(60, 412521, "1:54:35.35")]
	[DataRow(58.643, 5213, "01:28.89")]
	[DataRow(62.5, 11221, "02:59.54")]
	[DataRow(50, 6666, "02:13.32")]
	[DataRow(60.09, 325523, "1:30:17.26")]
	[DataRow(60, 16997, "04:43.28")]
	[DataRow(60, 0, "00:00.00")]
	[DataRow(0, 1, "10675199:02:48:05.47")]
	public void TimeableTimespanTest(double frameRate, int frames, string expected)
	{
		var actual = new Timeable { FrameRate = frameRate, Frames = frames }
			.Time().ToStringWithOptionalDaysAndHours();
		Assert.AreEqual(expected, actual);
	}
}
