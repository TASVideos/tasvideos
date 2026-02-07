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
	extension<T>(IQueryable<T> query)
		where T : ITrackable
	{
		public IQueryable<T> OldestToNewest() => query.OrderBy(t => t.CreateTimestamp);
		public IQueryable<T> ByMostRecent() => query.OrderByDescending(t => t.CreateTimestamp);
		public IQueryable<T> Since(DateTime target) => query.Where(t => t.CreateTimestamp >= target);
	}
}
