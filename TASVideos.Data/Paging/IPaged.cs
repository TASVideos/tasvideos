using System;

namespace TASVideos.Data
{
	public interface IPaged : IPageable
	{
		int RowCount { get; }
	}

	public static class PagedExtensions
	{
		public static int LastPage(this IPaged paged)
		{
			return (int)Math.Ceiling(paged.RowCount / (double)paged.PageSize);
		}

		public static int LastRow(this IPaged paged)
		{
			return Math.Min(paged.RowCount, paged.StartRow() + paged.PageSize - 1);
		} 
	}
}
