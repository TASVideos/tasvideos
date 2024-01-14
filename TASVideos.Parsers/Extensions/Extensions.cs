namespace TASVideos.MovieParsers.Extensions;

internal static class Extensions
{
	/// <summary>
	/// Splits by line, Null safe, removes empty entries.
	/// </summary>
	public static string[] LineSplit(this string? str)
	{
		if (str is null)
		{
			return Array.Empty<string>();
		}

		return str
			.Split(new[] { "\r\n", "\r", "\n" }, StringSplitOptions.RemoveEmptyEntries)
			.ToArray();
	}

	/// <summary>
	/// Searches through a list of strings that represents a space separated
	/// key/value pair, for the given key (case-insensitive and returns the value).
	/// </summary>
	/// <param name="lines">The key/value pairs to search.</param>
	/// <param name="key">The key to search for.</param>
	/// <returns>The value if found, else an empty string.</returns>
	public static string GetValueFor(this string[]? lines, string key)
	{
		if (lines is null || !lines.Any() || string.IsNullOrWhiteSpace(key))
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
		if (!result)
		{
			return null;
		}

		if (parsedVal >= 0)
		{
			return parsedVal;
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
		if (!result)
		{
			return null;
		}

		if (parsedVal >= 0)
		{
			return parsedVal;
		}

		return null;
	}

	/// <summary>
	/// Parses the given string as a boolean.
	/// </summary>
	/// <returns>True if value is a case-insensitive true, or a 1.</returns>
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
	/// Gets a file that matches or starts with the given name
	/// with a case-insensitive match.
	/// </summary>
	public static ZipArchiveEntry? Entry(this ZipArchive archive, string name)
	{
		return archive.Entries.SingleOrDefault(e => e.Name.ToLower().StartsWith(name));
	}

	public static bool HasEntry(this ZipArchive archive, string name)
	{
		return archive.Entries.Any(e => e.Name.ToLower() == name.ToLower());
	}

	// Returns a boolean indicating whether the given git is set in the given byte
	public static bool Bit(this byte b, int index)
	{
		return (b & (1 << index)) != 0;
	}

	/// <summary>
	/// Returns the header and frame count for a given stream of an input log. A frame here
	/// is defined as every line which starts with '|'. The header likewise is every line
	/// that is not a frame. Normally, this could be implemented with a combination of
	/// (await reader.ReadToEndAsync()).LineSplit() with .WithoutPipes() and .PipeCount().
	/// However, this ends up reading the entire (possibly decompressed) input log into a string.
	/// If the input log is >1GB big (assuming UTF8), it will end being "too big", as .NET disallows
	/// a single object being larger than 2GB (also, note .NET strings are UTF16 rather than UTF8).
	/// This method is used instead to figure out the frame count without loading the entire input log into memory.
	/// </summary>
	/// <param name="stream">stream of an input log</param>
	/// <returns>header and frame count</returns>
	public static async Task<(string[], int)> PipeBasedMovieHeaderAndFrameCount(this Stream stream)
	{
		using var reader = new StreamReader(stream);
		var frames = 0;
		var header = new List<string>();

		while (await reader.ReadLineAsync() is { } line)
		{
			if (line.StartsWith('|'))
			{
				frames++;
			}
			else
			{
				header.Add(line);
			}
		}

		return (header.ToArray(), frames);
	}
}
