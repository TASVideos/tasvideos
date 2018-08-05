using System.Collections.Generic;
using System.Linq;

namespace TASVideos.Data.Entity
{
	public interface ISoftDeletable
	{
		bool IsDeleted { get; }
	}

	public static class SoftDeleteQueryableExtensions
	{
		public static IQueryable<T> ThatAreNotDeleted<T>(this IQueryable<T> list)
			where T : class, ISoftDeletable
		{
			return list.Where(l => !l.IsDeleted);
		}

		public static IEnumerable<T> ThatAreNotDeleted<T>(this IEnumerable<T> list)
			where T : class, ISoftDeletable
		{
			return list.Where(l => !l.IsDeleted);
		}

		public static IQueryable<T> ThatAreDeleted<T>(this IQueryable<T> list)
			where T : class, ISoftDeletable
		{
			return list.Where(l => l.IsDeleted);
		}

		public static IEnumerable<T> ThatAreDeleted<T>(this IEnumerable<T> list)
			where T : class, ISoftDeletable
		{
			return list.Where(l => l.IsDeleted);
		}
	}
}
