using System;
using System.Linq;
using System.Reflection;

namespace TASVideos.Data
{
	public interface ISortable
	{
		/// <summary>
		/// Gets a comma separated list of fields to order by
		/// -/+ can be used to denote descending/ascending sort
		/// The default is ascending sort
		/// </summary>
		string Sort { get; }
	}

	public static class SortableExtensions
	{
		/// <summary>
		/// Returns whether or not the given parameter is specified
		/// </summary>
		public static bool IsSortingParam(this ISortable sortable, string param)
		{
			if (string.IsNullOrWhiteSpace(sortable?.Sort))
			{
				return false;
			}

			if (string.IsNullOrWhiteSpace(param))
			{
				return false;
			}

			return sortable.Sort
				.Split(new[] { "," }, StringSplitOptions.RemoveEmptyEntries)
				// ReSharper disable once StyleCop.SA1117
				.Any(str => string.Equals(str
					.Replace("-", "")
					.Replace("+", ""), param, StringComparison.OrdinalIgnoreCase));
		}

		/// <summary>
		/// Returns whether or not the given parameter is specified and
		/// is specified as a descending sort
		/// </summary>
		public static bool IsDescending(this ISortable sortable, string param)
		{
			if (string.IsNullOrWhiteSpace(sortable?.Sort))
			{
				return false;
			}

			if (string.IsNullOrWhiteSpace(param))
			{
				return false;
			}

			return sortable.Sort
				.Split(new[] { "," }, StringSplitOptions.RemoveEmptyEntries)
				// ReSharper disable once StyleCop.SA1117
				.Any(str => string.Equals(str
					.Replace("-", "")
					.Replace("+", ""),  param, StringComparison.OrdinalIgnoreCase)
					&& str.StartsWith("-"));
		}

		/// <summary>
		/// Returns whether or not the requested sort is valid based on the destination response
		/// The sorting is valid if all parameters match properties in the response, and that
		/// those properties are declared as sortable
		/// </summary>
		public static bool IsValidSort(this ISortable request, Type response)
		{
			if (string.IsNullOrWhiteSpace(request?.Sort) || response == null)
			{
				return true;
			}

			var requestedSorts = request.Sort
				.Split(new[] { "," }, StringSplitOptions.RemoveEmptyEntries)
				.Select(s => s.Replace("-", ""))
				.Select(s => s.Replace("+", ""))
				.Select(s => s.ToLower());

			var sortableProperties = response
				.GetProperties()
				.Where(p => p.GetCustomAttribute<SortableAttribute>() != null)
				.Select(p => p.Name.ToLower())
				.ToList();

			return requestedSorts.All(s => sortableProperties.Contains(s));
		}
	}
}
