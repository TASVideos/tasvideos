using System.Collections;
using System.Reflection;

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
	private static readonly Dictionary<Type, PagingDefaultsAttribute?> Defaults = [];

	public PagingModel()
	{
		var exists = Defaults.TryGetValue(GetType(), out var defaults);
		if (!exists)
		{
			defaults = GetType().GetCustomAttribute<PagingDefaultsAttribute>();
			Defaults[GetType()] = defaults;
		}

		if (defaults?.PageSize is not null)
		{
			PageSize = defaults.PageSize;
		}

		if (defaults?.Sort is not null)
		{
			Sort = defaults.Sort;
		}
	}

	public string? Sort { get; set; }
	public int? PageSize { get; init; } = 25;
	public int? CurrentPage { get; init; } = 1;
}

[AttributeUsage(AttributeTargets.Class)]
public class PagingDefaultsAttribute : Attribute
{
	public int PageSize { get; set; } = 25;
	public string? Sort { get; set; }
}
