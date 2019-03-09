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
		public static async Task<PageOf<T>> SortedPageOf<T>(this IQueryable<T> query, DbContext db, PagingModel paging)
			where T : class
		{
			return await query
				.SortBy(paging)
				.PageOf(db, paging);
		}

		public static async Task<PageOf<T>> PageOf<T>(
			this IQueryable<T> query,
			DbContext db,
			PagingModel paging)
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

				var pageOf = new PageOf<T>(results)
				{
					PageSize = paging.PageSize,
					CurrentPage = paging.CurrentPage,
					RowCount = rowCount,
					SortDescending = paging.SortDescending,
					SortBy = paging.SortBy
				};

				return pageOf;
			}
		}

		public static IQueryable<T> SortBy<T>(this IQueryable<T> query, ISortable sort)
		{
			if (string.IsNullOrWhiteSpace(sort?.SortBy))
			{
				return query;
			}

			string orderBy = sort.SortDescending
				? nameof(Enumerable.OrderByDescending)
				: nameof(Enumerable.OrderBy);

			// https://stackoverflow.com/questions/34899933/sorting-using-property-name-as-string
			// LAMBDA: x => x.[PropertyName]
			var parameter = Expression.Parameter(typeof(T), "x");
			Expression property = Expression.Property(parameter, sort.SortBy);
			var lambda = Expression.Lambda(property, parameter);

			var isSortable = typeof(T).GetProperty(sort.SortBy).GetCustomAttributes<SortableAttribute>().Any();
			if (!isSortable)
			{
				return query;
			}

			// REFLECTION: source.OrderBy(x => x.Property)
			var orderByMethod = typeof(Queryable).GetMethods().First(x => x.Name == orderBy && x.GetParameters().Length == 2);
			var orderByGeneric = orderByMethod.MakeGenericMethod(typeof(T), property.Type);
			var result = orderByGeneric.Invoke(null, new object[] { query, lambda });

			return (IOrderedQueryable<T>)result;
		}
	}
}
