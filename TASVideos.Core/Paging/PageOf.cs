using System.Collections;

namespace TASVideos.Core;

public class PageOf<T>(IEnumerable<T> items, PagingModel request) : PageOf<T, PagingModel>(items, request)
{
}

public class PageOf<T, T2>(IEnumerable<T> items, T2 request) : IPaged<T2>, IEnumerable<T>
	where T2 : IRequest
{
	public T2 Request { get; init; } = request;

	public int RowCount { get; init; }

	public IEnumerator<T> GetEnumerator() => items.GetEnumerator();
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
