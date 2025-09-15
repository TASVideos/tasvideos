using TASVideos.TagHelpers;
using TASVideos.WikiEngine.AST;

namespace TASVideos.RazorPages.Tests.ViewComponents;

[TestClass]
public class ParamHelperTests
{
	[TestMethod]
	[DataRow("", "anyParam", false)]
	[DataRow("\n \r \t", "anyParam", false)]
	[DataRow("id=1", "id", true)]
	[DataRow("id=", "id", true)]
	[DataRow(" id=", "id", true)]
	[DataRow("id =", "id", true)]
	[DataRow("name=test|id=1", "id", true)]
	[DataRow("name=|id=1", "id", true)]
	[DataRow("|||", "id", false)]
	[DataRow("name=|id=1", "ID", true)]
	[DataRow("name=|iD=1", "id", true)]
	[DataRow(" name = | iD =1", "id", true)]
	[DataRow("a", "a", true)]
	[DataRow("   ab    |  c    ", "c", true)]
	[DataRow("a|a", "a", true)]
	[DataRow("a|b|b|", "c", false)]
	public void HasParam(string parameterStr, string param, bool expected)
	{
		var moduleString = "foo|" + parameterStr;
		var module = new Module(0, moduleString.Length, moduleString);
		var actual = WikiMarkup.ConvertParameter<bool?>(module.Parameters.GetValueOrDefault(param));

		Assert.AreEqual(expected, actual);
	}

	[TestMethod]
	[DataRow("\r \n \t", "anyParam", null)]
	[DataRow("|||", "anyParam", null)]
	[DataRow("id=", "id", "")]
	[DataRow("id= ", "id", "")]
	[DataRow("id=1", "id", "1")]
	[DataRow("name=test|id=1", "id", "1")]
	[DataRow("name=test|id=1", "iD", "1")]
	[DataRow("| Id = 1 |", "iD", "1")]
	[DataRow("a=3|a=65", "a", "65")]
	public void GetValueFor(string parameterStr, string param, string expected)
	{
		var moduleString = "foo|" + parameterStr;
		var module = new Module(0, moduleString.Length, moduleString);
		var actual = WikiMarkup.ConvertParameter<string?>(module.Parameters.GetValueOrDefault(param));

		Assert.AreEqual(expected, actual);
	}

	[TestMethod]
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
	[DataRow("num =       5  ", "NUM", 5)]
	[DataRow("a=3|a=65", "a", 65)]
	public void GetInt(string parameterStr, string param, int? expected)
	{
		var moduleString = "foo|" + parameterStr;
		var module = new Module(0, moduleString.Length, moduleString);
		var actual = WikiMarkup.ConvertParameter<int?>(module.Parameters.GetValueOrDefault(param));

		Assert.AreEqual(expected, actual);
	}

	[TestMethod]
	[DataRow("num", "num", null)]
	[DataRow("num=", "num", null)]
	[DataRow("num=notnumber", "num", null)]
	[DataRow("num=1.1", "num", null)]
	[DataRow("num=0", "num", null)]
	[DataRow("num=-1", "num", null)]
	[DataRow("num=1", "num", null)]
	[DataRow("num=999999", "num", null)]
	[DataRow("| num = 1 |", "num", null)]
	[DataRow("num=1 1", "num", null)]
	[DataRow("num=Y", "num", null)]
	[DataRow("num=Y2014", "num", "2014-01-01")]
	[DataRow("num=foo    | num =     y2099", "num", "2099-01-01")]
	public void GetYear(string parameterStr, string param, string? expectedDt)
	{
		var moduleString = "foo|" + parameterStr;
		var module = new Module(0, moduleString.Length, moduleString);
		var actual = WikiMarkup.ConvertParameter<DateTime?>(module.Parameters.GetValueOrDefault(param));
		DateTime? expected = expectedDt != null ? DateTime.Parse(expectedDt) : null;

		Assert.AreEqual(expected, actual);
	}

	[TestMethod]
	[DataRow("csv", "csv", new int[0])]
	[DataRow("csv=", "csv", new int[0])]
	[DataRow("csv=notnumber", "csv", new int[0])]
	[DataRow("csv=1.1", "csv", new int[0])]
	[DataRow("csv=0", "csv", new[] { 0 })]
	[DataRow("csv=-1", "csv", new[] { -1 })]
	[DataRow("csv=1,2", "csv", new[] { 1, 2 })]
	[DataRow("csv=1, 2", "csv", new[] { 1, 2 })]
	[DataRow("csv=1,2,notnumber", "csv", new[] { 1, 2 })]
	[DataRow("a=3,4|a=5,6", "a", new[] { 5, 6 })]
	public void GetInts(string parameterStr, string param, int[] expected)
	{
		var moduleString = "foo|" + parameterStr;
		var module = new Module(0, moduleString.Length, moduleString);
		var actual = WikiMarkup.ConvertParameter<IList<int>>(module.Parameters.GetValueOrDefault(param));

		Assert.IsNotNull(actual);
		Assert.IsTrue(actual.SequenceEqual(expected));
	}
}
