using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Threading.Tasks;

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
				int rowsToSkip = paging.GetRowsToSkip();

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

		public static async Task<PageOf<T>> PageOfAsync<T>(
			this IOrderedQueryable<T> query,
			DbContext db,
			PagedModel paging)
			where T : class
		{
			using (await db.Database.BeginTransactionAsync())
			{
				int rowsToSkip = paging.GetRowsToSkip();

				int rowCount = await query.CountAsync();

				IEnumerable<T> results = await query
					.Skip(rowsToSkip)
					.Take(paging.PageSize)
					.ToListAsync();

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

		public static IOrderedQueryable<T> SortBy<T>(this IQueryable<T> query, PagingModel paging)
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
}
