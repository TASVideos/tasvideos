using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

using TASVideos.Common;

namespace TASVideos.Data
{
	public interface IPaged : IPageable, ISortable
	{
		int RowCount { get; }
	}

	public static class PagedExtensions
	{
		public static int LastPage(this IPaged paged)
		{
			var size = paged?.PageSize ?? 0;
			var count = paged?.RowCount ?? 0;
			if (count <= 0 || size <= 0)
			{
				return 0;
			}

			return (int)Math.Ceiling(count / (double)size);
		}

		public static int LastRow(this IPaged paged)
		{
			var size = paged?.PageSize ?? 0;
			var rowCount = paged?.RowCount ?? 0;
			return Math.Min(rowCount, paged.Offset() + size);
		} 

		public static IDictionary<string, string> AdditionalProperties(this IPaged paged)
		{
			if (paged == null)
			{
				return new Dictionary<string, string>();
			}

			var existing = typeof(IPaged)
				.GetProperties(BindingFlags.Public | BindingFlags.Instance)
				.Concat(typeof(IPaged)
					.GetInterfaces()
					.SelectMany(i => i.GetProperties()))
				.ToList();

			var existingNames = existing.Select(p => p.Name);

			var all = paged
				.GetType()
				.GetProperties(BindingFlags.Public | BindingFlags.Instance)
				.ToList();

			var additional = all
				.Where(p => !existingNames.Contains(p.Name))
				.ToList();

			return additional.ToDictionary(tkey => tkey.Name, tvalue => tvalue.ToValue(paged));
		}
	}
}
