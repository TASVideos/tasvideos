using System;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using TASVideos.WikiEngine;

namespace TASVideos.Test.ViewComponents
{
	[TestClass]
	public class ParamHelperTests
	{
		[TestMethod]
		[DataRow(null, "anyParam", false)]
		[DataRow("", "anyParam", false)]
		[DataRow("\n \r \t", "anyParam", false)]
		[DataRow("id=1", "id", true)]
		[DataRow("id=", "id", true)]
		[DataRow(" id=", "id", true)]
		[DataRow("id =", "id", true)]
		[DataRow("name=test|id=1", "id", true)]
		[DataRow("name=|id=1", "id", true)]
		[DataRow("id=1", null, false)]
		[DataRow("|=", "=", false)]
		[DataRow("|||", "id", false)]
		[DataRow("name=|id=1", "ID", true)]
		[DataRow("name=|iD=1", "id", true)]
		[DataRow(" name = | iD =1", "id", true)]
		[DataRow("a", "a", true)]
		[DataRow("   ab    |  c    ", "c", true)]
		public void HasParam(string parameterStr, string param, bool expected)
		{
			var actual = ParamHelper.HasParam(parameterStr, param);
			Assert.AreEqual(expected, actual);
		}

		[TestMethod]
		[DataRow(null, null, "")]
		[DataRow("", null, "")]
		[DataRow("\r \n \t", null, "")]
		[DataRow("\r \n \t", "anyParam", "")]
		[DataRow("|||", "anyParam", "")]
		[DataRow("name=test|id=1", null, "")]
		[DataRow("name=test|id=1", "", "")]
		[DataRow("name=test|id=1", "\r \n \t", "")]
		[DataRow("id=", "id", "")]
		[DataRow("id= ", "id", "")]
		[DataRow("id=1", "id", "1")]
		[DataRow("name=test|id=1", "id", "1")]
		[DataRow("name=test|id=1", "iD", "1")]
		[DataRow("| Id = 1 |", "iD", "1")]
		public void GetValueFor(string parameterStr, string param, string expected)
		{
			var actual = ParamHelper.GetValueFor(parameterStr, param);
			Assert.AreEqual(expected, actual);
		}

		[TestMethod]
		[DataRow(null, null, null)]
		[DataRow("", null, null)]
		[DataRow("\r \n \t", null, null)]
		[DataRow(null, "", null)]
		[DataRow(null, "\r \n \t", null)]
		[DataRow("num", "num", null)]
		[DataRow("num=", "num", null)]
		[DataRow("num=notnumber", "num", null)]
		[DataRow("num=1.1", "num", null)]
		[DataRow("num=0", "num", 0)]
		[DataRow("num=-1", "num", -1)]
		[DataRow("num=1", "num", 1)]
		[DataRow("num=999999", "num", 999999)]
		[DataRow("| num = 1 |", "num", 1)]
		[DataRow("num=1 1", "num", null)]
		public void GetInt(string parameterStr, string param, int? expected)
		{
			var actual = ParamHelper.GetInt(parameterStr, param);
			Assert.AreEqual(expected, actual);
		}

		[TestMethod]
		[DataRow(null, null, null)]
		[DataRow("", null, null)]
		[DataRow("\r \n \t", null, null)]
		[DataRow(null, "", null)]
		[DataRow(null, "\r \n \t", null)]
		[DataRow("num", "num", null)]
		[DataRow("num=", "num", null)]
		[DataRow("num=notnumber", "num", null)]
		[DataRow("num=1.1", "num", null)]
		[DataRow("num=0", "num", 0)]
		[DataRow("num=-1", "num", -1)]
		[DataRow("num=1", "num", 1)]
		[DataRow("num=999999", "num", 999999)]
		[DataRow("| num = 1 |", "num", 1)]
		[DataRow("num=1 1", "num", null)]
		[DataRow("num=Y", "num", null)]
		[DataRow("num=Y2014", "num", 2014)]
		public void GetYear(string parameterStr, string param, int? expected)
		{
			var actual = ParamHelper.GetYear(parameterStr, param);
			Assert.AreEqual(expected, actual);
		}

		[TestMethod]
		[DataRow(null, null, new int[0])]
		[DataRow("", null, new int[0])]
		[DataRow("\r \n \t", null, new int[0])]
		[DataRow(null, "", new int[0])]
		[DataRow(null, "\r \n \t", new int[0])]
		[DataRow("csv", "csv", new int[0])]
		[DataRow("csv=", "csv", new int[0])]
		[DataRow("csv=notnumber", "csv", new int[0])]
		[DataRow("csv=1.1", "csv", new int[0])]
		[DataRow("csv=0", "csv", new[] { 0 })]
		[DataRow("csv=-1", "csv", new[] { -1 })]
		[DataRow("csv=1,2", "csv", new[] { 1, 2 })]
		[DataRow("csv=1, 2", "csv", new[] { 1, 2 })]
		[DataRow("csv=1,2,notnumber", "csv", new[] { 1, 2 })]
		public void GetInts(string parameterStr, string param, int[] expected)
		{
			var actual = ParamHelper.GetInts(parameterStr, param);

			Assert.IsNotNull(actual);
			var actualList = actual.ToList();
			foreach (var expectedVal in expected)
			{
				Assert.IsTrue(actualList.Contains(expectedVal));
			}
		}
	}
}
