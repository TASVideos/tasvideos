﻿using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace TASVideos.Core
{
	public class PageOf<T> : IPaged, IEnumerable<T>
	{
		private readonly IEnumerable<T> _items;

		public PageOf(IEnumerable<T> items)
		{
			_items = items;
		}

		public int RowCount { get; set; }
		public string? Sort { get; set; }
		public int? PageSize { get; set; }
		public int? CurrentPage { get; set; }

		public IEnumerator<T> GetEnumerator() => _items.GetEnumerator();
		IEnumerator IEnumerable.GetEnumerator() => _items.GetEnumerator();

		public static PageOf<T> Empty() => new (Enumerable.Empty<T>());
	}

	/// <summary>
	/// Represents all of the data necessary to create a paged query.
	/// </summary>
	public class PagingModel : ISortable, IPageable
	{
		public string? Sort { get; set; }
		public int? PageSize { get; set; } = 10;
		public int? CurrentPage { get; set; } = 1;
	}
}
