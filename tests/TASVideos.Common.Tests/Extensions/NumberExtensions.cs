using TASVideos.Extensions;

namespace TASVideos.Common.Tests.Extensions;

[TestClass]
public class NumberExtensions
{
	[TestMethod]
	[DataRow(0, 0, 0, 0)]
	[DataRow(5, 1, 4, 4)]
	[DataRow(5, 6, 10, 6)]
	[DataRow(5, 0, 10, 5)]
	[DataRow(5, 10, 0, 5)]
	[DataRow(5, 4, 1, 4)]
	[DataRow(5, 1, 4, 4)]
	public void Clamp(int source, int start, int end, int expected)
	{
		var actual = source.Clamp(start, end);
		Assert.IsNotNull(actual);
		Assert.AreEqual(expected, actual);
	}

	[TestMethod]
	[DataRow(0, 0, 0.0)]
	[DataRow(1, 0, 0.0)]
	[DataRow(80, 100, 80.0)]
	[DataRow(2, 3, 66.7)]
	public void ToPercent(int source, int total, double expected)
	{
		var actual = source.ToPercent(total);
		Assert.AreEqual((decimal)expected, actual);
	}

	[TestMethod]
	[DataRow(0, "0 bytes")]
	[DataRow(-1, "-1 bytes")]
	[DataRow(1000, "1000 bytes")]
	[DataRow(1001, "1.00 KB")]
	[DataRow(1000_000, "1000.00 KB")]
	[DataRow(1000_001, "1.00 MB")]
	[DataRow(1000_000_000, "1000.00 MB")]
	[DataRow(1000_000_001, "1.00 GB")]
	[DataRow(1010_000_000, "1.01 GB")]
	[DataRow(1016_000_000, "1.02 GB")]
	public void ToSizeString(int byteCount, string expected)
	{
		var actual = byteCount.ToSizeString();
		Assert.AreEqual(expected, actual);
	}
}
