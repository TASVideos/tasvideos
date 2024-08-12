using System.Collections;

namespace TASVideos.Core;

public class PageOf<T>(IEnumerable<T> items, PagingModel request) : PageOf<T, PagingModel>(items, request)
{
}

public class PageOf<TItem, TRequest>(IEnumerable<TItem> items, TRequest request) : IPaged<TRequest>, IEnumerable<TItem>
	where TRequest : IRequest
{
	public TRequest Request { get; init; } = request;

	public int RowCount { get; init; }

	public IEnumerator<TItem> GetEnumerator() => items.GetEnumerator();
	IEnumerator IEnumerable.GetEnumerator() => items.GetEnumerator();
}

/// <summary>
/// Represents all the data necessary to create a paged query.
/// </summary>
public class PagingModel : IRequest
{
	public string? Sort { get; set; }
	public int? PageSize { get; set; } = 25;
	public int? CurrentPage { get; set; } = 1;
}
