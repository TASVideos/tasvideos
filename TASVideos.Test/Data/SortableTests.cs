using Microsoft.VisualStudio.TestTools.UnitTesting;
using TASVideos.Data;

// TODO: set naming rules separately for test project
// ReSharper disable InconsistentNaming
namespace TASVideos.Test.Data
{
	[TestClass]
	public class SortableTests
	{
		[TestMethod]
		public void SortableIsSortingParam_NullSafe()
		{
			var sortable = (ISortable)null;
			// ReSharper disable once ExpressionIsAlwaysNull
			var actual = sortable.IsSortingParam("Param");
			Assert.IsFalse(actual);
		}

		[TestMethod]
		[DataRow(null, null, false, DisplayName = "Null param always false")]
		[DataRow("", null, false, DisplayName = "Null param always false")]
		[DataRow(null, "", false, DisplayName = "Empty param always false")]
		[DataRow("", "", false, DisplayName = "Empty param always false")]
		[DataRow(null, "\r\n ", false, DisplayName = "Whitespace param always false")]
		[DataRow("", "\r\n ", false, DisplayName = "Whitespace param always false")]
		[DataRow(",", "", false)]
		[DataRow(" , ", " ", false)]
		[DataRow("", "Param", false)]
		[DataRow("Param", "Param", true)]
		[DataRow("+Param", "Param", true)]
		[DataRow("-Param", "Param", true)]
		[DataRow("-Param", "param", true, DisplayName = "Case insensitive")]
		[DataRow("-param", "param", true, DisplayName = "Case insensitive")]
		[DataRow("-parAm", "paRam", true, DisplayName = "Case insensitive")]
		public void Sortable_IsSortingParamTests(string sortStr, string param, bool expected)
		{
			var sortable = new Sortable(sortStr);
			var actual = sortable.IsSortingParam(param);
			Assert.AreEqual(expected, actual);
		}

		[TestMethod]
		public void SortableIsDescending_NullSafe()
		{
			var sortable = (ISortable)null;
			// ReSharper disable once ExpressionIsAlwaysNull
			var actual = sortable.IsDescending("Param");
			Assert.IsFalse(actual);
		}

		[TestMethod]
		[DataRow(null, null, false, DisplayName = "Null param always false")]
		[DataRow("", null, false, DisplayName = "Null param always false")]
		[DataRow(null, "", false, DisplayName = "Empty param always false")]
		[DataRow("", "", false, DisplayName = "Empty param always false")]
		[DataRow(null, "\r\n ", false, DisplayName = "Whitespace param always false")]
		[DataRow("", "\r\n ", false, DisplayName = "Whitespace param always false")]
		[DataRow(",", "", false)]
		[DataRow(" , ", " ", false)]
		[DataRow("", "Param", false)]
		[DataRow("Param", "Param", false)]
		[DataRow("+Param", "Param", false)]
		[DataRow("-Param", "Param", true)]
		[DataRow("-Param", "param", true, DisplayName = "Case insensitive")]
		[DataRow("-param", "param", true, DisplayName = "Case insensitive")]
		[DataRow("-parAm", "paRam", true, DisplayName = "Case insensitive")]
		public void Sortable_IsDescendingTests(string sortStr, string param, bool expected)
		{
			var sortable = new Sortable(sortStr);
			var actual = sortable.IsDescending(param);
			Assert.AreEqual(expected, actual);
		}

		private class Sortable : ISortable
		{
			public Sortable(string sort)
			{
				Sort = sort;
			}

			public string Sort { get; }
		}
	}
}
