namespace TASVideos.Core.Tests.Paging;

[TestClass]
public class PageableTests
{
	[TestMethod]
	public void Pageable_Offset_NullSafe()
	{
		var pageable = (IPageable?)null;

		// ReSharper disable once ExpressionIsAlwaysNull
		var actual = pageable.Offset();
		Assert.AreEqual(0, actual);
	}

	[TestMethod]
	[DataRow(null, null, 0)]
	[DataRow(null, 0, 0)]
	[DataRow(0, null, 0)]
	[DataRow(0, 0, 0)]
	[DataRow(25, 0, 0, DisplayName = "0 current treated as 1")]
	[DataRow(25, -1, 0, DisplayName = "Negative current treated as 1")]
	[DataRow(100, 1, 0)]
	[DataRow(100, 2, 100)]
	[DataRow(100, 3, 200)]
	public void Pageable_Offset_Tests(int? size, int? current, int expected)
	{
		var pageable = new Pageable(size, current);
		var actual = pageable.Offset();
		Assert.AreEqual(expected, actual);
	}

	private class Pageable : IPageable
	{
		public Pageable(int? pageSize, int? currentPage)
		{
			PageSize = pageSize;
			CurrentPage = currentPage;
		}

		public int? PageSize { get; }
		public int? CurrentPage { get; }
	}
}
