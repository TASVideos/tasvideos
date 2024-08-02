namespace TASVideos.Core;

public interface IPaged : IPageable, ISortable
{
	int RowCount { get; }

	IDictionary<string, string> AdditionalProperties();
}

public static class PagedExtensions
{
	public static int LastPage(this IPaged? paged)
	{
		var size = paged?.PageSize ?? 0;
		var count = paged?.RowCount ?? 0;
		if (count <= 0 || size <= 0)
		{
			return 0;
		}

		return (int)Math.Ceiling(count / (double)size);
	}

	public static int LastRow(this IPaged? paged)
	{
		var size = paged?.PageSize ?? 0;
		var rowCount = paged?.RowCount ?? 0;
		return Math.Min(rowCount, paged.Offset() + size);
	}
}
