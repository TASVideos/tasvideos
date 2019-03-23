using Microsoft.VisualStudio.TestTools.UnitTesting;
using TASVideos.ViewComponents;

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
		[DataRow("isFoo", "isFoo", null)]
		[DataRow("isFoo=", "isFoo", null)]
		[DataRow("isFoo=false", "isFoo", false)]
		[DataRow("isFoo=False", "isFoo", false)]
		[DataRow("isFoo=no", "isFoo", false)]
		[DataRow("isFoo=No", "isFoo", false)]
		[DataRow("isFoo=n", "isFoo", false)]
		[DataRow("isFoo=N", "isFoo", false)]
		[DataRow("isFoo=0", "isFoo", false)]
		[DataRow("isFoo=True", "isFoo", true)]
		[DataRow("isFoo=true", "isFoo", true)]
		[DataRow("isFoo=yes", "isFoo", true)]
		[DataRow("isFoo=Yes", "isFoo", true)]
		[DataRow("isFoo=Y", "isFoo", true)]
		[DataRow("isFoo=y", "isFoo", true)]
		[DataRow("isFoo=1", "isFoo", true)]
		[DataRow(" ISFoo = y ", "isfoo", true)]
		public void GetBool(string parameterStr, string param, bool? expected)
		{
			var actual = ParamHelper.GetBool(parameterStr, param);
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
	}
}
