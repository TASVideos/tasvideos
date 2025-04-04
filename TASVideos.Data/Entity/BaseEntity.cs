using TASVideos.Data.AutoHistory;

namespace TASVideos.Data.Entity;

public interface ITrackable
{
	DateTime CreateTimestamp { get; set; }
	DateTime LastUpdateTimestamp { get; set; }
}

public class BaseEntity : ITrackable
{
	[ExcludeFromAutoHistory]
	public DateTime CreateTimestamp { get; set; }

	[ExcludeFromAutoHistory]
	public DateTime LastUpdateTimestamp { get; set; }
}

public static class TrackableQueryableExtensions
{
	public static IQueryable<T> OldestToNewest<T>(this IQueryable<T> query)
		where T : ITrackable => query.OrderBy(t => t.CreateTimestamp);

	public static IQueryable<T> ByMostRecent<T>(this IQueryable<T> query)
		where T : ITrackable => query.OrderByDescending(t => t.CreateTimestamp);

	public static IQueryable<T> Since<T>(this IQueryable<T> query, DateTime target)
		where T : ITrackable => query.Where(t => t.CreateTimestamp >= target);
}
