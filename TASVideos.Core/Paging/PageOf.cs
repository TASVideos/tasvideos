using System.Collections;

namespace TASVideos.Core;

public class PageOf<T>(IEnumerable<T> items) : IPaged, IEnumerable<T>
{
	public int RowCount { get; set; }
	public string? Sort { get; set; }
	public int? PageSize { get; set; }
	public int? CurrentPage { get; set; }

	public IEnumerator<T> GetEnumerator() => items.GetEnumerator();
	IEnumerator IEnumerable.GetEnumerator() => items.GetEnumerator();

	public static PageOf<T> Empty() => new([]);
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
