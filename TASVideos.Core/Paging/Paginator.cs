﻿using System.Linq.Expressions;
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
			return source;
		}

		var columns = request.Sort.SplitWithEmpty(",").Select(s => s.Trim());

		bool thenBy = false;
		foreach (var column in columns)
		{
			source = SortByParam(source, column, thenBy);
			thenBy = true;
		}

		return source;
	}

	private static IQueryable<T> SortByParam<T>(IQueryable<T> query, string? column, bool thenBy)
	{
		bool desc = column?.StartsWith('-') ?? false;

		column = column?.Trim('-').Trim('+').ToLower() ?? "";

		var prop = typeof(T).GetProperties().FirstOrDefault(p => p.Name.ToLower() == column);

		if (prop is null)
		{
			return query;
		}

		if (prop.GetCustomAttribute(typeof(SortableAttribute)) is null)
		{
			return query;
		}

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
}
