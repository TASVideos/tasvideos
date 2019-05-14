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

		[TestMethod]
		[DataRow(null, null)]
		[DataRow("", "")]
		[DataRow("\r \n \t", "\r \n \t")]
		[DataRow(" ", " ")]
		[DataRow("onlylowercase", "onlylowercase")]
		[DataRow(" trimspaces ", "trimspaces")]
		[DataRow("If Spaces Do Not Add Extra Spaces", "If Spaces Do Not Add Extra Spaces")]
		[DataRow("HelloWorld", "Hello World")]
		[DataRow("ABTest", "AB Test")]
		[DataRow("ABCTest", "ABC Test")]
		[DataRow("ABCtest", "AB Ctest")]
		[DataRow("TASVideos", "TAS Videos")]
		[DataRow("Special.Characters.Get.Spaces", "Special . Characters . Get . Spaces")]
		[DataRow("GameResources/NES/SuperMarioBros", "Game Resources / NES / Super Mario Bros")]
		[DataRow("HomePages/adelikat", "Home Pages / adelikat")]
		[DataRow("HomePages/Adelikat", "Home Pages / Adelikat")]
		[DataRow("HomePages/[^_^]", "Home Pages / [^_^]")]
		public void SplitCamelCase_Tests(string str, string expected)
		{
			var actual = str.SplitCamelCase();
			Assert.AreEqual(expected, actual);
		}
	}
}
