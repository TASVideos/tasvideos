﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace TASVideos.Extensions
{
	public static class StringExtensions
	{
		/// <summary>
		/// Truncates the end of a string to the given character <see cref="limit"/> if the
		/// strength exceeds this limit, else the string itself is returned.
		/// If length exceeds limit, ellipses will be added to the result.
		/// </summary>
		public static string CapAndEllipse(this string? str, int limit)
		{
			if (str == null)
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
			if (str == null)
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

			var strings = str
				.SplitWithEmpty("/")
				.Select(s => s.SplitCamelCaseInternal());

			return string.Join(" / ", strings);
		}

		/// <summary>
		/// Takes a comma separated string and returns a list of values.
		/// </summary>
		public static IEnumerable<string> CsvToStrings(this string? param)
		{
			return string.IsNullOrWhiteSpace(param)
				? Enumerable.Empty<string>()
				: param
					.SplitWithEmpty(",")
					.Where(p => !string.IsNullOrWhiteSpace(p))
					.Select(p => p.Trim());
		}

		/// <summary>
		/// Takes a comma separated string and returns a list of values.
		/// </summary>
		public static IEnumerable<int> CsvToInts(this string? param)
		{
			if (string.IsNullOrWhiteSpace(param))
			{
				return Enumerable.Empty<int>();
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
			return str.Split(new[] { separator }, StringSplitOptions.RemoveEmptyEntries);
		}

		private static string SplitCamelCaseInternal(this string? str)
		{
			return Regex.Replace(
				Regex.Replace(
					str ?? "",
					@"(\P{Ll})(\P{Ll}\p{Ll})",
					"$1 $2"),
				@"(\p{Ll})(\P{Ll})",
				"$1 $2");
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

		public static string ToSnakeCase(this string str)
		{
			return string
				.Concat(str.Select((s, i) => i > 0 && char.IsUpper(s) ? "_" + s : s.ToString()))
				.ToLower();
		}
	}
}
