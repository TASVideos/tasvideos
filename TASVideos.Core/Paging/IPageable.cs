namespace TASVideos.Core;

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
	public static int Offset(this IPageable? pageable)
	{
		var current = Math.Max(pageable?.CurrentPage ?? 0, 1);
		var size = Math.Max(pageable?.PageSize ?? 0, 1);
		return (current - 1) * size;
	}
}
