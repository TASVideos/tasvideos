using System.Linq.Expressions;
using System.Reflection;

namespace TASVideos.Core;

public static class Paginator
{
	/// <summary>
	/// Takes an ordered query and returns a paged result set and a result count that has been executed.
	/// The <see cref="DbContext" /> is used to create a single transaction scope for both the query and count and execute those queries.
	/// </summary>
	/// <param name="query">The query to paginate and run.</param>
	/// <param name="paging">The paging data to use.</param>
	/// <typeparam name="TItem">The result type of the query.</typeparam>
	public static async Task<PageOf<TItem>> SortedPageOf<TItem>(this IQueryable<TItem> query, PagingModel paging)
		where TItem : class
	{
		return await query
			.SortBy(paging)
			.PageOf(paging);
	}

	public static async Task<PageOf<TItem, TRequest>> SortedPageOf<TItem, TRequest>(this IQueryable<TItem> query, TRequest paging)
		where TItem : class
		where TRequest : PagingModel
	{
		return await query
			.SortBy(paging)
			.PageOf(paging);
	}

	public static async Task<PageOf<TItem>> PageOf<TItem>(
		this IQueryable<TItem> query,
		PagingModel paging)
		where TItem : class
	{
		int rowsToSkip = paging.Offset();

		int rowCount = await query.CountAsync();

		var newQuery = query.Skip(rowsToSkip);

		if (paging.PageSize.HasValue)
		{
			int pageSize = Math.Max(paging.PageSize.Value, 1);
			newQuery = newQuery.Take(pageSize);
		}

		IEnumerable<TItem> results = await newQuery
			.ToListAsync();

		var pageOf = new PageOf<TItem>(results, paging)
		{
			RowCount = rowCount,
		};

		return pageOf;
	}

	public static async Task<PageOf<TItem, TRequest>> PageOf<TItem, TRequest>(
		this IQueryable<TItem> query,
		TRequest paging)
		where TItem : class
		where TRequest : PagingModel
	{
		int rowsToSkip = paging.Offset();

		int rowCount = await query.CountAsync();

		var newQuery = query.Skip(rowsToSkip);

		if (paging.PageSize.HasValue)
		{
			int pageSize = Math.Max(paging.PageSize.Value, 1);
			newQuery = newQuery.Take(pageSize);
		}

		IEnumerable<TItem> results = await newQuery
			.ToListAsync();

		var pageOf = new PageOf<TItem, TRequest>(results, paging)
		{
			RowCount = rowCount,
		};

		return pageOf;
	}

	/// <summary>
	/// Orders the given collection based on the <see cref="ISortable.Sort"/> property.
	/// </summary>
	/// <typeparam name="T">The type of the elements of source.</typeparam>
	public static IQueryable<T> SortBy<T>(this IQueryable<T> source, ISortable? request)
	{
		if (string.IsNullOrWhiteSpace(request?.Sort))
		{
			// per below, can't noop
			return ApplyDefaultSort(source);
		}

		var columns = request.Sort.SplitWithEmpty(",").Select(s => s.Trim());
		var anySortApplied = false;
		bool thenBy = false;
		foreach (var column in columns)
		{
			source = SortByParam(source, column, thenBy, out var sortApplied);
			anySortApplied |= sortApplied;
			thenBy = true;
		}

		// if we haven't added an `OrderBy` to the chain yet, we need to do that now or bad things happen
		// the caller is expecting us to, and if they go on to call `Take`/`Skip`, it hits UB
		return anySortApplied ? source : ApplyDefaultSort(source);
	}

	private static IQueryable<T> SortByParam<T>(IQueryable<T> query, string? column, bool thenBy, out bool sortApplied)
	{
		bool desc = column?.StartsWith('-') ?? false;

		column = column?.Trim('-').Trim('+').ToLower() ?? "";

		var prop = typeof(T).GetProperties().FirstOrDefault(p => p.Name.ToLower() == column);
		if (prop?.GetCustomAttribute<SortableAttribute>() is null)
		{
			sortApplied = false;
			return query;
		}

		sortApplied = true;
		return SortByParamInner(query, prop, column, desc: desc, thenBy: thenBy);
	}

	private static IQueryable<T> SortByParamInner<T>(IQueryable<T> query, PropertyInfo prop, string column, bool desc, bool thenBy)
	{
		string orderBy;
		if (thenBy)
		{
			orderBy = desc
				? nameof(Queryable.ThenByDescending)
				: nameof(Queryable.ThenBy);
		}
		else
		{
			orderBy = desc
				? nameof(Queryable.OrderByDescending)
				: nameof(Queryable.OrderBy);
		}

		// https://stackoverflow.com/questions/34899933/sorting-using-property-name-as-string
		// LAMBDA: x => x.[PropertyName]
		var parameter = Expression.Parameter(typeof(T), "x");
		Expression property = Expression.Property(parameter, column);
		var lambda = Expression.Lambda(property, parameter);

		// REFLECTION: source.OrderBy(x => x.Property)
		var orderByMethod = typeof(Queryable).GetMethods().First(x => x.Name == orderBy && x.GetParameters().Length == 2);
		var orderByGeneric = orderByMethod.MakeGenericMethod(typeof(T), property.Type);
		var result = orderByGeneric.Invoke(null, [query, lambda]);

		if (result is null)
		{
			return Enumerable.Empty<T>().AsQueryable();
		}

		return (IQueryable<T>)result;
	}

	private static IQueryable<T> ApplyDefaultSort<T>(IQueryable<T> query)
	{
		var allProps = typeof(T).GetProperties();
		var idProp = allProps.SingleOrDefault(x => string.Equals(x.Name, "id", StringComparison.OrdinalIgnoreCase));
		if (idProp?.GetCustomAttribute<SortableAttribute>() is not null)
		{
			return SortByParamInner(query, idProp, idProp.Name, desc: false, thenBy: false);
		}

		// worst case, just do everything
		var thenBy = false;
		foreach (var pi in allProps.Where(pi => pi.GetCustomAttribute<SortableAttribute>() is not null))
		{
			query = SortByParamInner(query, pi, pi.Name.ToLowerInvariant(), desc: false, thenBy: thenBy);
			thenBy = true;
		}

		return query;
	}
}
