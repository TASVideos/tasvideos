using System;
using System.Collections.Generic;
using System.Linq;

namespace TASVideos.Api
{
	/// <summary>
	/// API specific extension methods
	/// </summary>
	public static class Extensions
	{
		/// <summary>
		/// Takes a comma separated string and returns a list of values
		/// </summary>
		public static IEnumerable<string> CsvToStrings(this string param)
		{
			if (string.IsNullOrWhiteSpace(param))
			{
				return Enumerable.Empty<string>();
			}

			return param.Split(new[] { "," }, StringSplitOptions.RemoveEmptyEntries);
		}

		/// <summary>
		/// Takes a comma separated string and returns a list of values
		/// </summary>
		public static IEnumerable<int> CsvToInts(this string param)
		{
			if (string.IsNullOrWhiteSpace(param))
			{
				return Enumerable.Empty<int>();
			}

			var candidates = param.Split(new[] { "," }, StringSplitOptions.RemoveEmptyEntries);

			var ids = new List<int>();
			foreach (var candidate in candidates)
			{
				if (int.TryParse(candidate, out int parsed))
				{
					ids.Add(parsed);
				}
			}

			return ids;
		}

		/// <summary>
		/// Converts a start and end year into a list of years
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
}
