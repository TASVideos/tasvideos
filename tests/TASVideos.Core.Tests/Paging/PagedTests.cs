namespace TASVideos.Core.Tests.Paging;

[TestClass]
public class PagedTests
{
	[TestMethod]
	public void Paged_LastPage_NullSafe()
	{
		IPaged<IRequest>? paged = null;

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
		var paged = new Paged(count, new(size, 0));
		var actual = paged.LastPage();
		Assert.AreEqual(expected, actual);
	}

	[TestMethod]
	public void Paged_LastRow_NullSafe()
	{
		IPaged<IRequest>? paged = null;

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
		var paged = new Paged(count, new(size, current));
		var actual = paged.LastRow();
		Assert.AreEqual(expected, actual);
	}

	[TestMethod]
	public void Paged_AdditionalProperties_NullSafe()
	{
		IPaged<IRequest>? paged = null;

		// ReSharper disable once ExpressionIsAlwaysNull
		var actual = (paged?.Request).AdditionalProperties();
		Assert.AreEqual(0, actual.Count);
	}

	[TestMethod]
	public void Paged_AdditionalProperties_EmptyWhenNoAdditional()
	{
		var paged = new Paged(1, new(1, 1));
		var actual = paged.Request.AdditionalProperties();
		Assert.AreEqual(0, actual.Count);
	}

	[TestMethod]
	public void Paged_AdditionalProperties_StringParameter()
	{
		const string filterVal = "Test";
		var paged = new Paged(1, new StringRequest(1, 1)
		{
			StringFilter = filterVal
		});

		var actual = paged.Request.AdditionalProperties();
		Assert.AreEqual(1, actual.Count);
		Assert.AreEqual(filterVal, actual[nameof(StringRequest.StringFilter)]);
	}

	[TestMethod]
	public void Paged_AdditionalProperties_EnumerableParameter()
	{
		var paged = new Paged(1, new EnumerableRequest(1, 1)
		{
			IdList = [1, 2, 3]
		});

		var actual = paged.Request.AdditionalProperties();
		Assert.AreEqual(1, actual.Count);
		Assert.AreEqual("1|2|3", actual[nameof(EnumerableRequest.IdList)]);
	}

	private class Request(int? pageSize, int? currentPage) : IRequest
	{
		public int? PageSize { get; } = pageSize;
		public int? CurrentPage { get; } = currentPage;
		public string Sort => "";
	}

	private class StringRequest(int? pageSize, int? currentPage) : Request(pageSize, currentPage)
	{
		public string StringFilter { get; init; } = "";
	}

	private class EnumerableRequest(int? pageSize, int? currentPage) : Request(pageSize, currentPage)
	{
		public IEnumerable<int> IdList { get; init; } = [];
	}

	private class Paged(int rowCount, Request request) : IPaged<IRequest>
	{
		public int RowCount { get; } = rowCount;
		public IRequest Request { get; } = request;
	}
}
