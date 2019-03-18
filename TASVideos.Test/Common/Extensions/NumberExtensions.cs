using Microsoft.VisualStudio.TestTools.UnitTesting;
using TASVideos.Extensions;

namespace TASVideos.Test.Common.Extensions
{
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
		public void Clamp_Tests(int source, int start, int end, int expected)
		{
			var actual = source.Clamp(start, end);
			Assert.IsNotNull(actual);
			Assert.AreEqual(expected, actual);
		}
	}
}
