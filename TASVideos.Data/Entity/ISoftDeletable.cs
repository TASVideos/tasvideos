namespace TASVideos.Data.Entity;

public interface ISoftDeletable
{
	bool IsDeleted { get; }
}

public static class SoftDeleteQueryableExtensions
{
	extension<T>(IQueryable<T> query)
		where T : class, ISoftDeletable
	{
		public IQueryable<T> ThatAreNotDeleted() => query.Where(l => !l.IsDeleted);
		public IQueryable<T> ThatAreDeleted() => query.Where(l => l.IsDeleted);
	}

	public static IEnumerable<T> ThatAreDeleted<T>(this IEnumerable<T> list)
		where T : class, ISoftDeletable => list.Where(l => l.IsDeleted);
}
