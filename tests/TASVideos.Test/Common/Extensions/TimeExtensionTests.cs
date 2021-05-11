using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using TASVideos.Extensions;

namespace TASVideos.Test.Common.Extensions
{
	[TestClass]
	public class TimeExtensionTests
	{
		[TestMethod]
		[DataRow(0, 0, 0, 0, "0.00")]
		[DataRow(0, 0, 0, 0.11, "0.11")]
		[DataRow(0, 0, 0, 0.5, "0.50")]
		[DataRow(0, 0, 0, 1.0, "1.00")]
		[DataRow(0, 0, 1, 0, "1:00.00")]
		[DataRow(0, 0, 10, 0, "10:00.00")]
		[DataRow(0, 1, 0, 0, "1:00:00.00")]
		[DataRow(1, 0, 0, 0, "1.00:00:00.00")]
		public void ToCondensedString_Tests(int days, int hours, int minutes, double seconds, string expected)
		{
			var timeSpan = TimeSpan.FromSeconds(seconds + (minutes * 60) + (hours * 60 * 60) + (days * 60 * 60 * 24));
			var actual = timeSpan.ToCondensedString();
			Assert.AreEqual(expected, actual);
		}
	}
}
