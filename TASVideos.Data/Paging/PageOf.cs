using System;
using System.Collections;
using System.Collections.Generic;

namespace TASVideos.Data
{
	public class PageOf<T> : PagedModel, IEnumerable<T>
	{
		private readonly IEnumerable<T> _items;

		public PageOf(IEnumerable<T> items)
		{
			_items = items;
		}

		public IEnumerator<T> GetEnumerator() => _items.GetEnumerator();
		IEnumerator IEnumerable.GetEnumerator() => _items.GetEnumerator();
	}

	public class PagedModel : PagingModel
	{
		public int RowCount { get; set; }

		public int LastPage => (int)Math.Ceiling(RowCount / (double)PageSize);
		public int StartRow => ((CurrentPage - 1) * PageSize) + 1;
		public int LastRow => Math.Min(RowCount, StartRow + PageSize - 1);
	}

	/// <summary>
	/// Represents all of the data necessary to create a paged query
	/// </summary>
	public class PagingModel
	{
		// TODO: filtering?
		public string SortBy { get; set; } = "Id";
		public bool SortDescending { get; set; }
		public int PageSize { get; set; } = 10;
		public int CurrentPage { get; set; } = 1;
	}
}
