namespace TASVideos.Data
{
	/// <summary>
	/// Represents a request for a data collection that can be paged
	/// </summary>
	public interface IPageable
	{
		/// <summary>
		/// Gets the max number records to return
		/// </summary>
		int? PageSize { get; }

		/// <summary>
		/// Gets the page to start returning records
		/// </summary>
		int? CurrentPage { get; }
	}

	public static class PageableExtensions
	{
		public static int Offset(this IPageable pageable)
		{
			var current = pageable?.CurrentPage ?? 0;
			var size = pageable?.PageSize ?? 0;
			return ((current < 1 ? 1 : current) - 1) * size;
		}
	}
}
