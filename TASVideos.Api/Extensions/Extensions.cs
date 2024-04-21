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

	public static int? GetInt(this HttpRequest request, string key)
	{
		return int.TryParse(request.Query[key], out var val) ? val : null;
	}

	public static bool? GetBool(this HttpRequest request, string key)
	{
		return bool.TryParse(request.Query[key], out var val) ? val : null;
	}
}
