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
	}
}
