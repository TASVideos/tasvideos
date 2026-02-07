using SharpZipArchive = SharpCompress.Archives.Zip.ZipArchive;
using SharpZipArchiveEntry = SharpCompress.Archives.Zip.ZipArchiveEntry;

namespace TASVideos.MovieParsers.Extensions;

internal static class Extensions
{
	/// <summary>
	/// Splits by line, Null safe, removes empty entries.
	/// </summary>
	public static string[] LineSplit(this string? str)
		=> str is null
			? []
			: str.Split(["\r\n", "\r", "\n"], StringSplitOptions.RemoveEmptyEntries);

	/// <param name="lines">The key/value pairs to search.</param>
	extension(string[]? lines)
	{
		/// <summary>
		/// Searches through a list of strings that represents a space separated
		/// key/value pair, for the given key (case-insensitive and returns the value).
		/// </summary>
		/// <param name="key">The key to search for.</param>
		/// <returns>The value if found, else an empty string.</returns>
		public string GetValueFor(string key)
		{
			if (lines is null || !lines.Any() || string.IsNullOrWhiteSpace(key))
			{
				return "";
			}

			var row = lines.FirstOrDefault(l => l.StartsWith(key, StringComparison.InvariantCultureIgnoreCase))?.ToLower();
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

		public bool HasValue(string key)
		{
			if (lines is null || !lines.Any() || string.IsNullOrWhiteSpace(key))
			{
				return false;
			}

			var row = lines.FirstOrDefault(l => l.StartsWith(key, StringComparison.InvariantCultureIgnoreCase))?.ToLower();
			return !string.IsNullOrWhiteSpace(row);
		}

		/// <summary>
		/// Searches through a list of strings that represents a space separated
		/// key/value pair, for the given key (case-insensitive and returns the value) and parses as a boolean.
		/// </summary>
		/// <returns>True if value is a case-insensitive true, or a 1.</returns>
		public bool GetBoolFor(string key) => lines.GetValueFor(key).ToBool();

		/// <summary>
		/// Searches through a list of strings that represents a space separated
		/// and parses the resulting string as an integer.
		/// If value can not be parsed, null is returned.
		/// If the value is negative or greater than the max
		/// value of an int, null is returned.
		/// </summary>
		public int? GetPositiveIntFor(string key) => lines.GetValueFor(key).ToPositiveInt();

		/// <summary>
		/// Searches through a list of strings that represents a space separated
		/// and parses the resulting string as an integer.
		/// If value can not be parsed, null is returned.
		/// If the value is negative or greater than the max
		/// value of a long, null is returned.
		/// </summary>
		public long? GetPositiveLongFor(string key) => lines.GetValueFor(key).ToPositiveLong();
	}

	extension(string val)
	{
		internal int? ToPositiveInt()
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

		internal long? ToPositiveLong()
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

		internal bool ToBool()
		{
			if (string.IsNullOrWhiteSpace(val))
			{
				return false;
			}

			if (int.TryParse(val, out var parsedVal))
			{
				return parsedVal == 1;
			}

			return string.Equals(val, "true", StringComparison.InvariantCultureIgnoreCase);
		}
	}

	public static async Task<SharpZipArchive> OpenZipArchiveRead(this Stream stream)
	{
		// A seekable stream is required for SharpZipArchive.Open
		// Doing a copy here should be fairly cheap, and is fine for reading
		// (This is normally done implicitly in BCL's ZipArchive ctor in Read mode)
		var ms = new MemoryStream();
		await stream.CopyToAsync(ms);
		return SharpZipArchive.Open(ms);
	}

	extension(SharpZipArchive archive)
	{
		/// <summary>
		/// Gets a file that matches or starts with the given name
		/// with a case-insensitive match.
		/// </summary>
		public SharpZipArchiveEntry? Entry(string name)
			=> archive.Entries.SingleOrDefault(e => e.Key?.StartsWith(name, StringComparison.InvariantCultureIgnoreCase) == true);

		public bool HasEntry(string name)
			=> archive.Entries.Any(e => string.Equals(e.Key, name, StringComparison.InvariantCultureIgnoreCase));
	}

	// Returns a boolean indicating whether the given git is set in the given byte
	public static bool Bit(this byte b, int index) => (b & (1 << index)) != 0;

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
	public static async Task<(string[] Lines, int FrameCount)> PipeBasedMovieHeaderAndFrameCount(this Stream stream)
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
