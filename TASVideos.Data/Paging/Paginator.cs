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

				var newQuery = query.Skip(rowsToSkip);

				if (paging.PageSize.HasValue)
				{
					newQuery = newQuery.Take(paging.PageSize.Value);
				}

				IEnumerable<T> results = await newQuery
					.ToListAsync();

				var pageOf = new PageOf<T>(results)
				{
					PageSize = paging.PageSize,
					CurrentPage = paging.CurrentPage,
					RowCount = rowCount,
					Sort = paging.Sort
				};

				return pageOf;
			}
		}

		/// <summary>
		/// Orders the given collection based on the <see cref="ISortable.Sort"/> property
		/// </summary>
		/// <typeparam name="T">The type of the elements of source.</typeparam>
		public static IQueryable<T> SortBy<T>(this IQueryable<T> source, ISortable request)
		{
			if (string.IsNullOrWhiteSpace(request?.Sort))
			{
				return source;
			}

			var columns = request.Sort.Split(new[] { "," }, StringSplitOptions.RemoveEmptyEntries);

			bool thenBy = false;
			foreach (var column in columns)
			{
				source = SortByParam(source, column, thenBy);
				thenBy = true;
			}

			return source;
		}

		private static IQueryable<T> SortByParam<T>(IQueryable<T> query, string column, bool thenBy)
		{
			bool desc = column.StartsWith("-");

			column = column.Trim('-').Trim('+')?.ToLower();

			var prop = typeof(T).GetProperties().FirstOrDefault(p => p.Name.ToLower() == column);
			
			if (prop == null)
			{
				return query;
			}

			if (prop.GetCustomAttribute(typeof(SortableAttribute)) == null)
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
			Expression property = Expression.Property(parameter, column ?? "");
			var lambda = Expression.Lambda(property, parameter);

			// REFLECTION: source.OrderBy(x => x.Property)
			var orderByMethod = typeof(Queryable).GetMethods().First(x => x.Name == orderBy && x.GetParameters().Length == 2);
			var orderByGeneric = orderByMethod.MakeGenericMethod(typeof(T), property.Type);
			var result = orderByGeneric.Invoke(null, new object[] { query, lambda });

			return (IQueryable<T>)result;
		}
	}
}
