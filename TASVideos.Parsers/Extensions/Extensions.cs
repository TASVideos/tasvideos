using System;
using System.Linq;

namespace TASVideos.MovieParsers.Extensions
{
	public static class Extensions
	{
		/// <summary>
		/// Splits by line, Null safe, removes empty entries
		/// </summary>
		/// <param name="str"></param>
		/// <returns></returns>
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
	}
}
