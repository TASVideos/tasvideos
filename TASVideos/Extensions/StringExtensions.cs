namespace TASVideos.Extensions;

public static class StringExtensions
{
	public static ICollection<string> ToTokens(this string? routeQuery)
		=> string.IsNullOrWhiteSpace(routeQuery)
			? []
			: routeQuery
				.SplitWithEmpty("-")
				.Select(s => s.Trim(' '))
				.Where(s => !string.IsNullOrWhiteSpace(s))
				.Select(s => s.ToLower())
				.ToList();

	/// <param name="source">The list to search.</param>
	extension(IEnumerable<string> source)
	{
		/// <summary>
		/// Converts string lists such as [1S, 2S, 3S] to [1, 2, 3].
		/// </summary>
		/// <param name="suffix">The suffix (case in-sensitive) to parse.</param>
		/// <returns>A list of ints that were parsed from the list.</returns>
		public ICollection<int> ToIdList(char suffix)
			=> source
				.Where(t => t.ToLower().EndsWith(char.ToLower(suffix)))
				.Where(t => int.TryParse(t[..^1], out var unused))
				.Select(t => int.Parse(t[..^1]))
				.ToList();

		/// <summary>
		/// Converts string lists such as [Group1, Group2] to [1, 2].
		/// </summary>
		/// <param name="prefix">The suffix (case in-sensitive) to parse.</param>
		/// <returns>A list of ints that were parsed from the list.</returns>
		public ICollection<int> ToIdListPrefix(string prefix)
			=> source
				.Where(t => t.StartsWith(prefix, StringComparison.CurrentCultureIgnoreCase))
				.Select(s => s.Replace(prefix, ""))
				.Where(t => int.TryParse(t, out var unused))
				.Select(int.Parse)
				.ToList();
	}
}
