﻿using Microsoft.VisualStudio.TestTools.UnitTesting;
using TASVideos.Legacy.Imports;

namespace TASVideos.Legacy.Tests
{
	[TestClass]
	public class ImportHelperTests
	{
		[TestMethod]
		[DataRow(null, null)]
		[DataRow("", null)]
		[DataRow(" ", null)]
		[DataRow("\n", null)]
		[DataRow("?", null)] // phpbb2 is fun
		[DataRow("17e28998", "152.137.226.23")]
		[DataRow("20025f1830260000000000005f183026", "2002:5f18:3026::5f18:3026")]
		public void IpFromHex(string ipAddress, string expected)
		{
			var actual = ipAddress.IpFromHex();
			Assert.AreEqual(expected, actual);
		}
	}
}
