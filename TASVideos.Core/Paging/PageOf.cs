using System.Collections;
using System.Reflection;

namespace TASVideos.Core;

public class PageOf<T> : IPaged, IEnumerable<T>
{
	private readonly IEnumerable<T> _items;
	public PagingModel? Search { get; init; }

	public PageOf(IEnumerable<T> items, PagingModel? search = null)
	{
		_items = items;
		if (_items is PageOf<T> pageOf)
		{
			RowCount = pageOf.RowCount;
			Sort = pageOf.Sort;
			PageSize = pageOf.PageSize;
			CurrentPage = pageOf.CurrentPage;
		}

		Search = search;
	}

	public int RowCount { get; init; }
	public string? Sort { get; init; }
	public int? PageSize { get; init; }
	public int? CurrentPage { get; init; }

	public IEnumerator<T> GetEnumerator() => _items.GetEnumerator();
	IEnumerator IEnumerable.GetEnumerator() => _items.GetEnumerator();

	public IDictionary<string, string> AdditionalProperties()
	{
		if (Search is null)
		{
			return new Dictionary<string, string>();
		}

		var existing = typeof(IPaged)
			.GetProperties(BindingFlags.Public | BindingFlags.Instance)
			.Concat(typeof(IPaged)
				.GetInterfaces()
				.SelectMany(i => i.GetProperties()))
			.ToList();

		var existingNames = existing.Select(p => p.Name);

		var all = Search
			.GetType()
			.GetProperties(BindingFlags.Public | BindingFlags.Instance)
			.ToList();
		var additional = all
		.Where(p => !existingNames.Contains(p.Name))
			.ToList();

		return additional.ToDictionary(tkey => tkey.Name, tvalue => tvalue.ToValue(Search));
	}
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
