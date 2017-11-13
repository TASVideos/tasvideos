using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore;

namespace TASVideos.Data
{
	public static class Paginator
	{
		/// <summary>
		/// Takes an ordered query and returns a paged result set and a result count that has been executed.
		/// Note that type extended is an <see cref="IOrderedQueryable"/>, Entity Framework requires a query to be ordered prior to skip and take methods
		/// The <see cref="DbContext" /> is used to create a single transaction scope for both the query and count and execute those queries.
		/// </summary>
		/// <param name="query">The query to paginate and run</param>
		/// <param name="db">The Entity Framework context instance</param>
		/// <param name="currentPage">The current page</param>
		/// <param name="pageSize">The size of each page</param>
		/// <param name="rowCount">The total result set count before paging</param>
		/// <typeparam name="T">The result type of the query</typeparam>
		public static IEnumerable<T> Paginate<T>( // TODO: async?
			this IOrderedQueryable<T> query,
			DbContext db,
			int currentPage,
			int pageSize,
			out int rowCount)
			where T : class
		{
			int rowsToSkip = ((currentPage < 1 ? 1 : currentPage) - 1) * pageSize;

			IEnumerable<T> results;

			using (db.Database.BeginTransaction())
			{
				rowCount = query.Count();

				results = query
					.Skip(rowsToSkip)
					.Take(pageSize)
					.ToList();
			}

			return results;
		}
	}
}
