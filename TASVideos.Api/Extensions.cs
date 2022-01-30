namespace TASVideos.Api;

/// <summary>
/// API specific extension methods.
/// </summary>
public static class Extensions
{
	/// <summary>
	/// Converts a start and end year into a list of years.
	/// </summary>
	public static IEnumerable<int> YearRange(this int? start, int? end)
	{
		if (!start.HasValue && !end.HasValue)
		{
			return Enumerable.Empty<int>();
		}

		var startYear = start ?? 2000; // Well before any TAS movies were created
		var endYear = end ?? 2050; // Well after the life expectancy of this code

		return Enumerable.Range(startYear, endYear - endYear + 1);
	}
}
