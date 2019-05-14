using System;
using System.Linq;
using System.Text.RegularExpressions;

namespace TASVideos.Extensions
{
	public static class StringExtensions
	{
		/// <summary>
		/// Truncates the end of a string to the given character <see cref="limit"/> if the 
		/// strength exceeds this limit, else the string itself is returned.
		/// If length exceeds limit, ellipses will be added to the result
		/// </summary>
		public static string CapAndEllipse(this string str, int limit)
		{
			if (str == null)
			{
				return null;
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

			return str.Substring(0, limit - 3) + "...";
		}

		/// <summary>
		/// Takes a string and adds spaces between words,
		/// As well as forward slashes
		/// Also accounts for acronyms
		/// </summary>
		public static string SplitCamelCase(this string str)
		{
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

			var strs = str
				.Split(new[] { "/" }, StringSplitOptions.RemoveEmptyEntries)
				.Select(s => s.SplitCamelCaseInternal());

			return string.Join(" / ", strs);
		}

		private static string SplitCamelCaseInternal(this string str)
		{
			return Regex.Replace(
				Regex.Replace(
					str,
					@"(\P{Ll})(\P{Ll}\p{Ll})",
					"$1 $2"
				),
				@"(\p{Ll})(\P{Ll})",
				"$1 $2"
			);
		}
	}
}
