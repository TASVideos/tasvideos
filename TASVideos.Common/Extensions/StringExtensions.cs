using System.Text.RegularExpressions;

namespace TASVideos.Extensions;

public ref struct RegexMatchShim
{
	private const string ErrMsgZeroed = $"this {nameof(RegexMatchShim)} has no value";

	private GroupCollection? _groups = null;

	private readonly Regex _regex;

	private readonly ReadOnlySpan<char> _str;

	private readonly Range _valueRange;

	public GroupCollection Groups
		=> Success
			? (_groups ??= _regex.Match(_str.ToString()).Groups)
			: throw new InvalidOperationException(ErrMsgZeroed);

	public readonly bool Success;

	public readonly ReadOnlySpan<char> Value
		=> Success
			? _str[_valueRange]
			: throw new InvalidOperationException(ErrMsgZeroed);

	internal RegexMatchShim(Regex regex, ReadOnlySpan<char> str, Range valueRange)
	{
		_regex = regex;
		_str = str;
		_valueRange = valueRange;
		Success = true;
	}
}

public static partial class StringExtensions
{
	/// <summary>
	/// Truncates the end of a string to the given character <see cref="limit"/> if the
	/// strength exceeds this limit, else the string itself is returned.
	/// If length exceeds limit, ellipses will be added to the result.
	/// </summary>
	public static string CapAndEllipse(this string? str, int limit)
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

	public static string? Cap(this string? str, int limit)
	{
		if (str is null)
		{
			return null;
		}

		return str.Length < limit
			? str
			: str[..limit];
	}

	public static bool EndsWithAny(this ReadOnlySpan<char> str, IEnumerable<string> suffixes)
	{
		foreach (var sfx in suffixes)
		{
			if (str.EndsWith(sfx))
			{
				return true;
			}
		}

		return false;
	}

	public static RegexMatchShim Match(this Regex regex, ReadOnlySpan<char> str)
	{
		var iter = regex.EnumerateMatches(str);
		return iter.MoveNext()
			? new(regex, str, iter.Current.Index..(iter.Current.Index + iter.Current.Length))
			: default;
	}

	/// <summary>
	/// Takes a string and adds spaces between words,
	/// As well as forward slashes
	/// Also accounts for acronyms.
	/// </summary>
	public static string SplitCamelCase(this string? str)
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

		if (string.IsNullOrWhiteSpace(str))
		{
			return str;
		}

		return str.SplitCamelCaseInternal();
	}

	/// <summary>
	/// Uses <see cref="SplitCamelCase"/> to split the string,
	/// but also checks for a HomePages path and undoes the split for the usernames in the path.
	/// </summary>
	public static string SplitPathCamelCase(this string? str)
	{
		if (str is null)
		{
			return "";
		}

		if (!str.StartsWith("HomePages/"))
		{
			return SplitCamelCase(str);
		}

		var pathFragments = str.SplitWithEmpty("/");
		for (int i = 0; i < pathFragments.Length; i++)
		{
			if (i != 1)
			{
				pathFragments[i] = SplitCamelCase(pathFragments[i]);
			}
		}

		return string.Join(" / ", pathFragments);
	}

	/// <summary>
	/// Takes a comma separated string and returns a list of values.
	/// </summary>
	public static ICollection<string> CsvToStrings(this string? param)
	{
		return string.IsNullOrWhiteSpace(param)
			? []
			: param
				.SplitWithEmpty(",")
				.Where(p => !string.IsNullOrWhiteSpace(p))
				.Select(p => p.Trim())
				.ToList();
	}

	/// <summary>
	/// Takes a comma separated string and returns a list of values.
	/// </summary>
	public static ICollection<int> CsvToInts(this string? param)
	{
		if (string.IsNullOrWhiteSpace(param))
		{
			return [];
		}

		var candidates = param.CsvToStrings();

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

	public static string[] SplitWithEmpty(this string str, string separator)
	{
		return str.Split([separator], StringSplitOptions.RemoveEmptyEntries);
	}

	private static readonly Regex SplitCamelCaseRegex = SplitCamelCaseCompiledRegex();
	private static string SplitCamelCaseInternal(this string? str)
	{
		return !string.IsNullOrWhiteSpace(str)
			? SplitCamelCaseRegex.Replace(str, "$1$2$3$4$5 ")
			: "";
	}

	public static bool StartsWithAny(this ReadOnlySpan<char> str, IEnumerable<string> prefixes)
	{
		foreach (var pfx in prefixes)
		{
			if (str.StartsWith(pfx))
			{
				return true;
			}
		}

		return false;
	}

	private static readonly Regex SpaceRegex = SpaceCompiledRegex();
	public static string RemoveAllSpaces(this string? str)
	{
		return SpaceRegex.Replace(str ?? "", "");
	}

	public static string UnicodeAwareSubstring(this string s, int startIndex)
	{
		return UnicodeAwareSubstring(s, startIndex, s.Length - startIndex);
	}

	public static string UnicodeAwareSubstring(this string s, int startIndex, int length)
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

	public static string LastCommaToAmpersand(this string commaString)
	{
		int lastComma = commaString.LastIndexOf(',');

		if (lastComma == -1)
		{
			return commaString;
		}

		return commaString[..lastComma] + " &" + commaString[(lastComma + 1)..];
	}

	/// <summary>
	/// Replaces all types of newlines with spaces.
	/// Multiple newlines will be replaced with multiple spaces.
	/// </summary>
	public static string NewlinesToSpaces(this string s)
	{
		return NewLinesToSpacesRegex().Replace(s, " ");
	}

	/// <summary>
	/// If the string is null, empty, or white space, null is returned.
	/// Else the original string is returned
	/// </summary>
	public static string? NullIfWhitespace(this string? s)
	{
		return string.IsNullOrWhiteSpace(s)
			? null
			: s;
	}

	/// <summary>
	/// Replaces the first occurrence of the given string.
	/// </summary>
	public static string ReplaceFirst(this string? text, string search, string replace)
	{
		if (text is null)
		{
			return "";
		}

		int pos = text.IndexOf(search);
		if (pos < 0)
		{
			return text;
		}

		return string.Concat(text.AsSpan(0, pos), replace, text.AsSpan(pos + search.Length));
	}

	public static string PascalToCamelCase(this string str)
	{
		if (string.IsNullOrWhiteSpace(str))
		{
			return str;
		}

		return char.ToLowerInvariant(str[0]) + str[1..];
	}

	/// <summary>
	/// Replaces the characters &lt; and &gt; with their fullwidth versions ＜ and ＞, because YouTube doesn't allow the regular symbols in titles and descriptions.
	/// </summary>
	public static string FormatForYouTube(this string s)
	{
		return s.Replace('<', '＜').Replace('>', '＞');
	}

	public static List<string> RemoveEmpty(this IEnumerable<string> strList)
	{
		return strList.Where(a => !string.IsNullOrWhiteSpace(a)).ToList();
	}

	[GeneratedRegex(@"(\/)|(\p{Ll})(?=[\p{Lu}\p{Nd}])|(\p{Nd})(?=[\p{Lu}])|([\p{L}\p{Nd}])(?=[^\p{L}\p{Nd}])|([^\p{L}\p{Nd}])(?=[\p{L}\p{Nd}])")]
	private static partial Regex SplitCamelCaseCompiledRegex();

	[GeneratedRegex(" +")]
	private static partial Regex SpaceCompiledRegex();

	[GeneratedRegex(@"\r\n?|\n")]
	private static partial Regex NewLinesToSpacesRegex();
}
