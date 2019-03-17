using System;
using System.Collections;
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

		private static string ToValue(this PropertyInfo property, object obj)
		{
			if (obj == null || property == null)
			{
				return null;
			}

			if (typeof(IEnumerable).IsAssignableFrom(property.PropertyType)
				&& property.PropertyType.IsGenericType)
			{
				var values = ((IEnumerable)property.GetValue(obj)).Cast<object>();
				var val = string.Join(",", values);
				return val;
			}

			return property.GetValue(obj)?.ToString();
		}
	}
}
