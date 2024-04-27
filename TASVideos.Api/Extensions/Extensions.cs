using System.Globalization;
using System.Reflection;

namespace TASVideos.Api;

internal static class Extensions
{
	public static IEnumerable<int> YearRange(this int? start, int? end)
	{
		if (!start.HasValue && !end.HasValue)
		{
			return [];
		}

		var startYear = start ?? 2000; // Well before any TAS movies were created
		var endYear = end ?? 2050; // Well after the life expectancy of this code

		return Enumerable.Range(startYear, endYear - endYear + 1);
	}

	/// <summary>
	/// Returns whether the requested sort is valid based on the destination response
	/// The sorting is valid if all parameters match properties in the response, and that
	/// those properties are declared as sortable.
	/// </summary>
	public static bool IsValidSort(this string requestedSort, Type? response)
	{
		if (response is null)
		{
			return false;
		}

		if (string.IsNullOrWhiteSpace(requestedSort))
		{
			return true;
		}

		var requestedSorts = requestedSort
			.SplitWithEmpty(",")
			.Select(str => str.Trim())
			.Select(s => s.Replace("-", ""))
			.Select(s => s.Replace("+", ""))
			.Select(s => s.ToLower(CultureInfo.InvariantCulture));

		var sortableProperties = response
			.GetProperties()
			.Where(p => p.GetCustomAttribute<SortableAttribute>() != null)
			.Select(p => p.Name.ToLower(CultureInfo.InvariantCulture))
			.ToList();

		return requestedSorts.All(s => sortableProperties.Contains(s));
	}
}
