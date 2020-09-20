using System;
using System.Collections.Generic;
using System.Linq;

namespace TASVideos.Pages
{
	public static class Extensions
	{
		public static ICollection<string> ToTokens(this string? routeQuery)
		{
			if (string.IsNullOrWhiteSpace(routeQuery))
			{
				return new List<string>();
			}

			return routeQuery
				.Split(new[] { "-" }, StringSplitOptions.RemoveEmptyEntries)
				.Select(s => s.Trim(' '))
				.Where(s => !string.IsNullOrWhiteSpace(s))
				.Select(s => s.ToLower())
				.ToList();
		}

		/// <summary>
		/// Converts string lists such as [1S, 2S, 3S] to [1, 2, 3]
		/// </summary>
		/// <param name="suffix">The suffix (case in-sensitive) to parse</param>
		/// <returns>A list of ints that were able to be parsed from the list</returns>
		public static ICollection<int> ToIdList(this IEnumerable<string> source, char suffix)
		{
			return source
				.Where(t => t.ToLower().EndsWith(char.ToLower(suffix)))
				.Where(t => int.TryParse(t[0..^1], out int unused))
				.Select(t => int.Parse(t[0..^1]))
				.ToList();
		}
	}
}
