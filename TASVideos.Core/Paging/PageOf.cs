using System.Collections;

namespace TASVideos.Core;

public class PageOf<T> : IPaged, IEnumerable<T>
{
	private readonly IEnumerable<T> _items;

	public PageOf(IEnumerable<T> items)
	{
		_items = items;
		if (_items is PageOf<T> pageOf)
		{
			RowCount = pageOf.RowCount;
			Sort = pageOf.Sort;
			PageSize = pageOf.PageSize;
			CurrentPage = pageOf.CurrentPage;
		}
	}

	public int RowCount { get; init; }
	public string? Sort { get; init; }
	public int? PageSize { get; init; }
	public int? CurrentPage { get; init; }

	public IEnumerator<T> GetEnumerator() => _items.GetEnumerator();
	IEnumerator IEnumerable.GetEnumerator() => _items.GetEnumerator();
}

/// <summary>
/// Represents all the data necessary to create a paged query.
/// </summary>
public class PagingModel : ISortable, IPageable
{
	public string? Sort { get; set; }
	public int? PageSize { get; set; } = 25;
	public int? CurrentPage { get; set; } = 1;
}
