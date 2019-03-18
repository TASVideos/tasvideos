using System;

using Microsoft.VisualStudio.TestTools.UnitTesting;
using TASVideos.Extensions;

namespace TASVideos.Test.Common.Extensions
{
	[TestClass]
	public class StringExtensionTests
	{
		[TestMethod]
		[ExpectedException(typeof(ArgumentException))]
		public void CapAndEllipse_NegativeLimit_Throws()
		{
			"".CapAndEllipse(-1);
		}

		[TestMethod]
		[DataRow(null, 0, null)]
		[DataRow(null, 1, null)]
		[DataRow("", 0, "")]
		[DataRow("", 1, "")]
		[DataRow("1234", 1, ".")]
		[DataRow("1234", 2, "..")]
		[DataRow("123", 3, "123")]
		[DataRow("1234", 4, "1234")]
		[DataRow("123456789", 7, "1234...")]
		[DataRow("123456789", 8, "12345...")]
		[DataRow("123456789", 9, "123456789")]
		[DataRow("123456789", 15, "123456789")]
		public void CapAndEllipse_Tests(string str, int limit, string expected)
		{
			var actual = str.CapAndEllipse(limit);
			Assert.AreEqual(expected, actual);
		}
	}
}
