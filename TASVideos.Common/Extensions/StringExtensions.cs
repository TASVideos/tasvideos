using System.Text.RegularExpressions;

namespace TASVideos.Extensions;

public static partial class StringExtensions
{
	extension(string? str)
	{
		/// <summary>
		/// Truncates the end of a string to the given character <see cref="limit"/> if the
		/// strength exceeds this limit, else the string itself is returned.
		/// If length exceeds limit, ellipses will be added to the result.
		/// </summary>
		public string CapAndEllipse(int limit)
		{
			if (str is null)
			{
				return "";
			}

			if (limit < 0)
			{
				throw new ArgumentException($"{nameof(limit)} cannot be less than zero");
			}

			if (limit == 0)
			{
				return "";
			}

			if (str.Length <= limit)
			{
				return str;
			}

			if (limit <= 3)
			{
				return new string('.', limit);
			}

			return str.UnicodeAwareSubstring(0, limit - 3) + "...";
		}

		public string? Cap(int limit)
		{
			if (str is null)
			{
				return null;
			}

			return str.Length < limit
				? str
				: str[..limit];
		}

		/// <summary>
		/// Takes a string and adds spaces between words,
		/// As well as forward slashes
		/// Also accounts for acronyms.
		/// </summary>
		public string SplitCamelCase()
		{
			if (string.IsNullOrEmpty(str))
			{
				return "";
			}

			if (string.IsNullOrWhiteSpace(str))
			{
				return str;
			}

			str = str
				.Trim()
				.Replace(" ", "");

			return string.IsNullOrWhiteSpace(str)
				? str
				: str.SplitCamelCaseInternal();
		}

		/// <summary>
		/// Uses <see cref="SplitCamelCase"/> to split the string,
		/// but also checks for a HomePages path and undoes the split for the usernames in the path.
		/// </summary>
		public string SplitPathCamelCase()
		{
			if (str is null)
			{
				return "";
			}

			if (!str.StartsWith("HomePages/"))
			{
				return str.SplitCamelCase();
			}

			var pathFragments = str.SplitWithEmpty("/");
			for (var i = 0; i < pathFragments.Length; i++)
			{
				if (i != 1)
				{
					pathFragments[i] = pathFragments[i].SplitCamelCase();
				}
			}

			return string.Join(" / ", pathFragments);
		}

		/// <summary>
		/// Takes a comma separated string and returns a list of values.
		/// </summary>
		public ICollection<string> CsvToStrings()
			=> string.IsNullOrWhiteSpace(str)
				? []
				: str
					.SplitWithEmpty(",")
					.Where(p => !string.IsNullOrWhiteSpace(p))
					.Select(p => p.Trim())
					.ToList();

		/// <summary>
		/// Takes a comma separated string and returns a list of values.
		/// </summary>
		public ICollection<int> CsvToInts()
		{
			if (string.IsNullOrWhiteSpace(str))
			{
				return [];
			}

			var candidates = str.CsvToStrings();

			var ids = new List<int>();
			foreach (var candidate in candidates)
			{
				if (int.TryParse(candidate, out var parsed))
				{
					ids.Add(parsed);
				}
			}

			return ids;
		}

		/// <summary>
		/// Normalizes a comma-separated list of values by trimming whitespace around each value.
		/// Returns null if the result would be empty or whitespace-only.
		/// </summary>
		public string? NormalizeCsv()
		{
			if (string.IsNullOrWhiteSpace(str))
			{
				return null;
			}

			var authors = str.SplitWithEmpty(",")
				.Select(author => author.Trim())
				.Where(author => !string.IsNullOrEmpty(author));

			var result = string.Join(",", authors);
			return string.IsNullOrWhiteSpace(result) ? null : result;
		}

		public string RemoveAllSpaces() => SpaceRegex.Replace(str ?? "", "");

		public string[] SplitWithEmpty(string separator)
			=> str is null
				? []
				: str.Split([separator], StringSplitOptions.RemoveEmptyEntries);

		/// <summary>
		/// If the string is null, empty, or white space, null is returned.
		/// Else the original string is returned
		/// </summary>
		public string? NullIfWhitespace()
		{
			return string.IsNullOrWhiteSpace(str)
				? null
				: str;
		}

		/// <summary>
		/// Replaces the first occurrence of the given string.
		/// </summary>
		public string ReplaceFirst(string search, string replace)
		{
			if (str is null)
			{
				return "";
			}

			var pos = str.IndexOf(search);
			return pos < 0
				? str
				: string.Concat(str.AsSpan(0, pos), replace, str.AsSpan(pos + search.Length));
		}

		public string PascalToCamelCase()
		{
			if (string.IsNullOrWhiteSpace(str))
			{
				return str ?? "";
			}

			return char.ToLowerInvariant(str[0]) + str[1..];
		}

		private string SplitCamelCaseInternal()
			=> !string.IsNullOrWhiteSpace(str)
				? SplitCamelCaseRegex.Replace(str, "$1$2$3$4$5 ")
				: "";
	}

	extension(string s)
	{
		public string UnicodeAwareSubstring(int startIndex, int length)
		{
			if ((uint)startIndex > s.Length)
			{
				throw new ArgumentOutOfRangeException(nameof(startIndex));
			}

			var endIndex = startIndex + length;
			if (endIndex < startIndex || endIndex > s.Length)
			{
				throw new ArgumentOutOfRangeException(nameof(length));
			}

			if (startIndex == s.Length || endIndex == 0)
			{
				return "";
			}

			if (char.IsLowSurrogate(s[startIndex]))
			{
				if (--startIndex < 0 || !char.IsHighSurrogate(s[startIndex]))
				{
					throw new InvalidOperationException("Unpaired Low Surrogate");
				}
			}

			if (char.IsHighSurrogate(s[endIndex - 1]))
			{
				if (++endIndex >= s.Length || !char.IsLowSurrogate(s[endIndex - 1]))
				{
					throw new InvalidOperationException("Unpaired High Surrogate");
				}
			}

			return s[startIndex..endIndex];
		}

		public string LastCommaToAmpersand()
		{
			var lastComma = s.LastIndexOf(',');

			if (lastComma == -1)
			{
				return s;
			}

			return s[..lastComma] + " &" + s[(lastComma + 1)..];
		}

		/// <summary>
		/// Replaces all types of newlines with spaces.
		/// Multiple newlines will be replaced with multiple spaces.
		/// </summary>
		public string NewlinesToSpaces() => NewLinesToSpacesRegex().Replace(s, " ");

		/// <summary>
		/// Replaces the characters &lt; and &gt; with their fullwidth versions ＜ and ＞, because YouTube doesn't allow the regular symbols in titles and descriptions.
		/// </summary>
		public string FormatForYouTube() => s.Replace('<', '＜').Replace('>', '＞');

		public string RemoveUrls() => SimpleUrlsRegex().Replace(s, "");
	}

	public static List<string> RemoveEmpty(this IEnumerable<string> strList) => [.. strList.Where(a => !string.IsNullOrWhiteSpace(a))];

	private static readonly Regex SplitCamelCaseRegex = SplitCamelCaseCompiledRegex();

	private static readonly Regex SpaceRegex = SpaceCompiledRegex();

	[GeneratedRegex(@"(\/)|(\p{Ll})(?=[\p{Lu}\p{Nd}])|(\p{Nd})(?=[\p{Lu}])|([\p{L}\p{Nd}])(?=[^\p{L}\p{Nd}])|([^\p{L}\p{Nd}])(?=[\p{L}\p{Nd}])")]
	private static partial Regex SplitCamelCaseCompiledRegex();

	[GeneratedRegex(" +")]
	private static partial Regex SpaceCompiledRegex();

	[GeneratedRegex(@"\r\n?|\n")]
	private static partial Regex NewLinesToSpacesRegex();
	[GeneratedRegex(@"https?:\/\/\S*")]
	private static partial Regex SimpleUrlsRegex();
}
