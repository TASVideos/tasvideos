using System;
using System.Collections;
using System.Collections.Generic;
using System.IO.Compression;
using System.Linq;

namespace TASVideos.MovieParsers.Extensions
{
	public static class Extensions
	{
		/// <summary>
		/// Splits by line, Null safe, removes empty entries
		/// </summary>
		public static string[] LineSplit(this string str)
		{
			if (str == null)
			{
				return new string[0];
			}

			return str.Split(
				new[] { '\n' },
				StringSplitOptions.RemoveEmptyEntries);
		}

		/// <summary>
		/// Searches through a list of strings that represents a space separated
		/// key/value pair, for the given key (case insensitive and returns the value
		/// </summary>
		/// <param name="lines">The key/value pairs to search</param>
		/// <param name="key">The key to search for</param>
		/// <returns>The value if found, else an empty string</returns>
		public static string GetValueFor(this string[] lines, string key)
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
		/// Parses the given string as an integer.
		/// If value can not be parsed, null is returned
		/// </summary>
		public static int? ToInt(this string val)
		{
			var result = int.TryParse(val, out var parsedVal);
			if (result)
			{
				return parsedVal;
			}

			return null;
		}

		/// <summary>
		/// Parses the given string as a boolean.
		/// </summary>
		/// <returns>True if value is a case insensitive true, or a 1</returns>
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
		/// an input frame in many movie formats
		/// </summary>
		public static int PipeCount(this IEnumerable<string> lines)
		{
			if (lines == null)
			{
				return 0;
			}

			return lines.Count(i => i.StartsWith("|"));
		}

		/// <summary>
		/// Returns lines that do not begin with a | which indicates
		/// a header line in many movie formats;
		/// </summary>
		public static IEnumerable<string> WithoutPipes(this IEnumerable<string> lines)
		{
			if (lines == null)
			{
				return Enumerable.Empty<string>();
			}

			return lines.Where(i => !i.StartsWith("|"));
		}

		/// <summary>
		/// Gets a file that matches or starts with the given name
		/// with a case insensitive match
		/// </summary>
		public static ZipArchiveEntry Entry(this ZipArchive archive, string name)
		{
			return archive.Entries.SingleOrDefault(e => e.Name.ToLower().StartsWith(name));
		}

		// Returns a boolean indicating whether or not the given git is set in the given byte
		public static bool Bit(this byte b, int index)
		{
			return (b & (1 << index)) != 0;
		}
	}
}
