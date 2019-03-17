using System.Collections.Generic;

using Microsoft.VisualStudio.TestTools.UnitTesting;
using TASVideos.Data;

// TODO: fix project level settings
// ReSharper disable InconsistentNaming
namespace TASVideos.Test.Data.Paging
{
	[TestClass]
	public class PagedTests
	{
		[TestMethod]
		public void Paged_LastPage_NullSafe()
		{
			var paged = (IPaged)null;
			// ReSharper disable once ExpressionIsAlwaysNull
			var actual = paged.LastPage();
			Assert.AreEqual(0, actual);
		}

		[TestMethod]
		[DataRow(0, null, 0)]
		[DataRow(0, 0, 0)]
		[DataRow(1, 0, 0)]
		[DataRow(-1, 0, 0)]
		[DataRow(-1, -1, 0)]
		[DataRow(1, 1, 1)]
		[DataRow(int.MaxValue, int.MaxValue, 1)]
		[DataRow(25, 25, 1)]
		[DataRow(100, 25, 4)]
		[DataRow(101, 25, 5)]
		public void Paged_LastPage_Tests(int count, int? size, int expected)
		{
			var paged = new Paged(count, size, 0);
			var actual = paged.LastPage();
			Assert.AreEqual(expected, actual);
		}

		[TestMethod]
		public void Paged_LastRow_NullSafe()
		{
			var paged = (IPaged)null;
			// ReSharper disable once ExpressionIsAlwaysNull
			var actual = paged.LastRow();
			Assert.AreEqual(0, actual);
		}

		[TestMethod]
		[DataRow(0, 0, 0, 0)]
		[DataRow(null, 0, 0, 0)]
		[DataRow(0, null, 0, 0)]
		[DataRow(0, 0, null, 0)]
		[DataRow(1, 0, 0, 0)]
		[DataRow(1, 1, 0, 1)]
		[DataRow(500, 100, 1, 100)]
		[DataRow(500, 100, 2, 200)]
		[DataRow(500, 100, 5, 500)]
		[DataRow(500, 100, 6, 500, DisplayName = "Pages beyond the max are capped")]
		[DataRow(401, 100, 5, 401)]
		public void Paged_LastRow_Tests(int count, int? size, int? current, int expected)
		{
			var paged = new Paged(count, size, current);
			var actual = paged.LastRow();
			Assert.AreEqual(expected, actual);
		}

		[TestMethod]
		public void Paged_AdditionalProperties_NullSafe()
		{
			var paged = (IPaged)null;
			// ReSharper disable once ExpressionIsAlwaysNull
			var actual = paged.AdditionalProperties();
			Assert.IsNotNull(actual);
			Assert.AreEqual(0, actual.Count);
		}

		[TestMethod]
		public void Paged_AdditionalProperties_EmptyWhenNoAdditional()
		{
			var paged = new Paged(1, 1, 1);
			var actual = paged.AdditionalProperties();
			Assert.IsNotNull(actual);
			Assert.AreEqual(0, actual.Count);
		}

		[TestMethod]
		public void Paged_AdditionalProperties_StringParameter()
		{
			var filterVal = "Test";
			var paged = new TestPagedModel(1, 1, 1)
			{
				StringFilter = filterVal
			};

			var actual = paged.AdditionalProperties();
			Assert.IsNotNull(actual);
			Assert.AreEqual(1, actual.Count);
			Assert.AreEqual(filterVal, actual[nameof(TestPagedModel.StringFilter)]);
		}

		[TestMethod]
		public void Paged_AdditionalProperties_EnumerableParameter()
		{
			var paged = new EnumerablePagedModel(1, 1, 1)
			{
				IdList = new List<int> { 1, 2, 3 }
			};

			var actual = paged.AdditionalProperties();
			Assert.IsNotNull(actual);
			Assert.AreEqual(1, actual.Count);
			Assert.AreEqual("1|2|3", actual[nameof(EnumerablePagedModel.IdList)]);
		}

		private class Paged : IPaged
		{
			public Paged(int rowCount, int? pageSize, int? currentPage)
			{
				RowCount = rowCount;
				PageSize = pageSize;
				CurrentPage = currentPage;
			}

			public int RowCount { get; }
			public int? PageSize { get; }
			public int? CurrentPage { get; }
			public string Sort => "";
		}

		private class TestPagedModel : Paged
		{
			public TestPagedModel(int rowCount, int? pageSize, int? currentPage)
				: base(rowCount, pageSize, currentPage)
			{
			}

			// ReSharper disable once UnusedAutoPropertyAccessor.Local
			public string StringFilter { get; set; }
		}

		private class EnumerablePagedModel : Paged
		{
			public EnumerablePagedModel(int rowCount, int? pageSize, int? currentPage)
				: base(rowCount, pageSize, currentPage)
			{
			}

			// ReSharper disable once UnusedAutoPropertyAccessor.Local
			public IEnumerable<int> IdList { get; set; }
		}
	}
}
