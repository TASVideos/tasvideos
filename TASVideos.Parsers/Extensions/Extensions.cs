namespace TASVideos.MovieParsers.Extensions;

internal static class Extensions
{
	/// <summary>
	/// Splits by line, Null safe, removes empty entries.
	/// </summary>
	public static string[] LineSplit(this string? str)
	{
		if (str == null)
		{
			return Array.Empty<string>();
		}

		return str
			.Split(new[] { "\r\n", "\r", "\n" }, StringSplitOptions.RemoveEmptyEntries)
			.ToArray();
	}

	/// <summary>
	/// Searches through a list of strings that represents a space separated
	/// key/value pair, for the given key (case insensitive and returns the value.
	/// </summary>
	/// <param name="lines">The key/value pairs to search.</param>
	/// <param name="key">The key to search for.</param>
	/// <returns>The value if found, else an empty string.</returns>
	public static string GetValueFor(this string[]? lines, string key)
	{
		if (lines == null || !lines.Any() || string.IsNullOrWhiteSpace(key))
		{
			return "";
		}

		var row = lines.FirstOrDefault(l => l.ToLower().StartsWith(key.ToLower()))?.ToLower();
		if (!string.IsNullOrWhiteSpace(row))
		{
			var valStr = row
				.Replace(key.ToLower(), "")
				.Trim()
				.Replace("\r", "")
				.Replace("\n", "");

			return valStr;
		}

		return "";
	}

	/// <summary>
	/// Receives a space separate key/value pair and returns the value as a string
	/// </summary>
	public static string GetValue(this string str)
	{
		if (string.IsNullOrWhiteSpace(str))
		{
			return "";
		}

		var split = str.Split(new[] { " " }, StringSplitOptions.RemoveEmptyEntries);
		if (split.Length < 2)
		{
			return "";
		}

		return split[1].Trim();
	}

	/// <summary>
	/// Parses the given string as an integer.
	/// If value can not be parsed, null is returned.
	/// If the value is negative or greater than the max
	/// value of an int, null is returned.
	/// </summary>
	public static int? ToPositiveInt(this string val)
	{
		var result = int.TryParse(val, out var parsedVal);
		if (result)
		{
			if (parsedVal >= 0)
			{
				return parsedVal;
			}
		}

		return null;
	}

	/// <summary>
	/// Parses the given string as a long.
	/// If value can not be parsed, null is returned.
	/// </summary>
	public static long? ToPositiveLong(this string val)
	{
		var result = long.TryParse(val, out var parsedVal);
		if (result)
		{
			if (parsedVal >= 0)
			{
				return parsedVal;
			}
		}

		return null;
	}

	/// <summary>
	/// Parses the given string as a boolean.
	/// </summary>
	/// <returns>True if value is a case insensitive true, or a 1.</returns>
	public static bool ToBool(this string val)
	{
		if (string.IsNullOrWhiteSpace(val))
		{
			return false;
		}

		if (int.TryParse(val, out int parsedVal))
		{
			return parsedVal == 1;
		}

		return string.Equals(val, "true", StringComparison.InvariantCultureIgnoreCase);
	}

	/// <summary>
	/// Returns the number of lines that start with a | which indicates
	/// an input frame in many movie formats.
	/// </summary>
	public static int PipeCount(this IEnumerable<string>? lines)
	{
		return lines?.Count(i => i.StartsWith("|")) ?? 0;
	}

	/// <summary>
	/// Returns lines that do not begin with a | which indicates
	/// a header line in many movie formats.
	/// </summary>
	public static IEnumerable<string> WithoutPipes(this IEnumerable<string>? lines)
	{
		return lines == null
			? Enumerable.Empty<string>()
			: lines.Where(i => !i.StartsWith("|"));
	}

	/// <summary>
	/// Gets a file that matches or starts with the given name
	/// with a case insensitive match.
	/// </summary>
	public static ZipArchiveEntry? Entry(this ZipArchive archive, string name)
	{
		return archive.Entries.SingleOrDefault(e => e.Name.ToLower().StartsWith(name));
	}

	// Returns a boolean indicating whether or not the given git is set in the given byte
	public static bool Bit(this byte b, int index)
	{
		return (b & (1 << index)) != 0;
	}
}
