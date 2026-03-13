namespace TASVideos.Common.Tests;

[TestClass]
public class TimeableTests
{
	[TestMethod]
	[DataRow(60.1, 421, "00:07.005")]
	[DataRow(75, 2346, "00:31.280")]
	[DataRow(60, 412521, "1:54:35.350")]
	[DataRow(58.643, 5213, "01:28.894")]
	[DataRow(62.5, 11221, "02:59.536")]
	[DataRow(50, 6666, "02:13.320")]
	[DataRow(60.09, 325523, "1:30:17.257")]
	[DataRow(60, 16997, "04:43.283")]
	[DataRow(60, 0, "00:00.001")]
	[DataRow(0, 1, "10675199:02:48:05.477")]
	public void TimeableTimespanTest(double frameRate, int frames, string expected)
	{
		var actual = new Timeable { FrameRate = frameRate, Frames = frames }
			.Time().ToStringWithOptionalDaysAndHours();
		Assert.AreEqual(expected, actual);
	}

	[TestMethod]
	[DataRow(-259200, "3 days ago")]
	[DataRow(-172800, "2 days ago")]
	[DataRow(-172799, "Yesterday")]
	[DataRow(-86400, "Yesterday")]
	[DataRow(-86399, "23 hours ago")]
	[DataRow(-7200, "2 hours ago")]
	[DataRow(-7199, "1 hour ago")]
	[DataRow(-3600, "1 hour ago")]
	[DataRow(-3599, "59 minutes ago")]
	[DataRow(-120, "2 minutes ago")]
	[DataRow(-119, "1 minute ago")]
	[DataRow(-61, "1 minute ago")]
	[DataRow(-60, "1 minute ago")]
	[DataRow(-59, "59 seconds ago")]
	[DataRow(-30, "30 seconds ago")]
	[DataRow(-5, "5 seconds ago")]
	[DataRow(-4, "Now")]
	[DataRow(0, "Now")]
	[DataRow(4, "Now")]
	[DataRow(5, "In 5 seconds")]
	[DataRow(30, "In 30 seconds")]
	[DataRow(59, "In 59 seconds")]
	[DataRow(60, "In 1 minute")]
	[DataRow(61, "In 1 minute")]
	[DataRow(119, "In 1 minute")]
	[DataRow(120, "In 2 minutes")]
	[DataRow(3599, "In 59 minutes")]
	[DataRow(3600, "In 1 hour")]
	[DataRow(7199, "In 1 hour")]
	[DataRow(7200, "In 2 hours")]
	[DataRow(86399, "In 23 hours")]
	[DataRow(86400, "Tomorrow")]
	[DataRow(172799, "Tomorrow")]
	[DataRow(172800, "In 2 days")]
	[DataRow(259200, "In 3 days")]
	public void ToRelativeString(int seconds, string expected)
	{
		var timeSpan = TimeSpan.FromSeconds(seconds);

		var result = timeSpan.ToRelativeString();

		Assert.AreEqual(expected, result);
	}
}
