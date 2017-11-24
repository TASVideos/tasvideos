using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using Microsoft.EntityFrameworkCore;

namespace TASVideos.Data
{
	public static class Paginator
	{
		/// <summary>
		/// Takes an ordered query and returns a paged result set and a result count that has been executed.
		/// The <see cref="DbContext" /> is used to create a single transaction scope for both the query and count and execute those queries.
		/// </summary>
		/// <param name="query">The query to paginate and run</param>
		/// <param name="db">The Entity Framework context instance</param>
		/// <param name="paging">The paging data to use</param>
		/// <typeparam name="T">The result type of the query</typeparam>
		public static PageOf<T> SortedPageOf<T>(this IQueryable<T> query, DbContext db, PagedModel paging)
			where T : class
		{
			return query
				.SortBy(paging)
				.PageOf(db, paging);
		}
		
		/// <summary>
		/// Takes an ordered query and returns a paged result set and a result count that has been executed.
		/// Note that type extended is an <see cref="IOrderedQueryable"/>, Entity Framework requires a query to be ordered prior to skip and take methods
		/// The <see cref="DbContext" /> is used to create a single transaction scope for both the query and count and execute those queries.
		/// </summary>
		/// <param name="query">The query to paginate and run</param>
		/// <param name="db">The Entity Framework context instance</param>
		/// <param name="paging">The paging data to use</param>
		/// <typeparam name="T">The result type of the query</typeparam>
		public static PageOf<T> PageOf<T>( // TODO: async?
			this IOrderedQueryable<T> query,
			DbContext db,
			PagedModel paging)
			where T : class
		{
			using (db.Database.BeginTransaction())
			{
				int rowsToSkip = ((paging.CurrentPage < 1 ? 1 : paging.CurrentPage) - 1) * paging.PageSize;

			    int rowCount = query.Count();

				IEnumerable<T> results = query
				    .Skip(rowsToSkip)
				    .Take(paging.PageSize)
				    .ToList();

				var pageof = new PageOf<T>(results)
				{
					PageSize = paging.PageSize,
					CurrentPage = paging.CurrentPage,
					RowCount = rowCount,
					SortDescending = paging.SortDescending,
					SortBy = paging.SortBy
				};

				return pageof;
			}
		}

		public static IOrderedQueryable<T> SortBy<T>(this IQueryable<T> query, IPagingModel paging)
		{
			string orderby = paging.SortDescending
				? nameof(Enumerable.OrderByDescending)
				: nameof(Enumerable.OrderBy);

			// https://stackoverflow.com/questions/34899933/sorting-using-property-name-as-string
			// LAMBDA: x => x.[PropertyName]
			var parameter = Expression.Parameter(typeof(T), "x");
			Expression property = Expression.Property(parameter, paging.SortBy);
			var lambda = Expression.Lambda(property, parameter);

			var isSortable = typeof(T).GetProperty(paging.SortBy).GetCustomAttributes<SortableAttribute>().Any();
			if (!isSortable)
			{
				throw new InvalidOperationException($"Attempted to sort by non-sortable column {paging.SortBy}");
			}

			// REFLECTION: source.OrderBy(x => x.Property)
			var orderByMethod = typeof(Queryable).GetMethods().First(x => x.Name == orderby && x.GetParameters().Length == 2);
			var orderByGeneric = orderByMethod.MakeGenericMethod(typeof(T), property.Type);
			var result = orderByGeneric.Invoke(null, new object[] { query, lambda });

			return (IOrderedQueryable<T>)result;
		}
	}

	public class SortableAttribute : Attribute
	{
	}

	public interface IPagingModel
	{
		string SortBy { get; }
		bool SortDescending { get; }
		int PageSize { get; }
		int CurrentPage { get; }
	}

	public class PageOf<T> : PagedModel, IEnumerable<T>
	{
		private readonly IEnumerable<T> _items;

		public PageOf(IEnumerable<T> items)
		{
			_items = items;
		}

		public IEnumerator<T> GetEnumerator() => _items.GetEnumerator();
		IEnumerator IEnumerable.GetEnumerator() => _items.GetEnumerator();
	}

	public class PagedModel : PagingModel
	{
		public int RowCount { get; set; }

		public int LastPage => (int)Math.Ceiling(RowCount / (double)PageSize);
		public int StartRow => ((CurrentPage - 1) * PageSize) + 1;
		public int LastRow => Math.Min(RowCount, StartRow + PageSize - 1);
	}

	/// <summary>
	/// Represents all of the data necessary to create a paged query
	/// </summary>
	public class PagingModel : IPagingModel
	{
		// TODO: filtering?
		public string SortBy { get; set; } = "Id";
		public bool SortDescending { get; set; }
		public int PageSize { get; set; } = 10;
		public int CurrentPage { get; set; } = 1;
	}
}
