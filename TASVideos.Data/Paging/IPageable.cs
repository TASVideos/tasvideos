namespace TASVideos.Data
{
	public interface IPageable
	{
		int PageSize { get; }
		int CurrentPage { get; }
	}

	public static class PageableExtensions
	{
		public static int GetRowsToSkip(this IPageable pageable)
		{
			return ((pageable.CurrentPage < 1 ? 1 : pageable.CurrentPage) - 1) * pageable.PageSize;
		}

		public static int StartRow(this IPageable pageable)
		{
			return ((pageable.CurrentPage - 1) * pageable.PageSize) + 1;
		}
	}
}
