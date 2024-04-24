namespace TASVideos.Core;

/// <summary>
/// Represents a request for a data collection that can be sorted.
/// </summary>
public interface ISortable
{
	/// <summary>
	/// Gets a comma separated list of fields to order by
	/// -/+ can be used to denote descending/ascending sort
	/// The default is ascending sort.
	/// </summary>
	string? Sort { get; }
}

public static class SortableExtensions
{
	/// <summary>
	/// Returns whether the given parameter is specified.
	/// </summary>
	public static bool IsSortingParam(this ISortable? sortable, string? param)
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
			.SplitWithEmpty(",")
			.Select(str => str.Trim())
			.Any(str => string.Equals(
				str.Replace("-", "").Replace("+", ""),
				param,
				StringComparison.OrdinalIgnoreCase));
	}

	/// <summary>
	/// Returns whether the given parameter is specified and
	/// is specified as a descending sort.
	/// </summary>
	public static bool IsDescending(this ISortable? sortable, string? param)
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
			.SplitWithEmpty(",")
			.Select(str => str.Trim())
			.Any(str => string.Equals(
					str.Replace("-", "").Replace("+", ""),
					param,
					StringComparison.OrdinalIgnoreCase)
				&& str.StartsWith('-'));
	}
}
