using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

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
			return (int)Math.Ceiling(paged.RowCount / (double)paged.PageSize);
		}

		public static int LastRow(this IPaged paged)
		{
			return Math.Min(paged.RowCount, paged.StartRow() + paged.PageSize - 1);
		} 

		public static IDictionary<string, string> AdditionalProperties(this IPaged paged)
		{
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

			var myDic = additional.ToDictionary(tkey => tkey.Name, tvalue => tvalue.GetValue(paged)?.ToString());

			return myDic;
		}
	}
}
