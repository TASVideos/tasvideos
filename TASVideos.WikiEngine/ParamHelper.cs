using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace TASVideos.WikiEngine
{
	/// <summary>
	/// Helpers for parsing parameters in wiki markup.
	/// By design they need to gracefully handle all sorts of nonsense input from the user.
	/// </summary>
	public static class ParamHelper
	{
		private static readonly Regex ValidParameterNameRegex = new Regex("^[a-zA-Z]+$");

		private static string NormalizeParameterName(string parameterName)
		{
			if (!ValidParameterNameRegex.IsMatch(parameterName))
				throw new ArgumentException($"Parameter name `{parameterName}` is invalid", nameof(parameterName));
			return parameterName.ToLowerInvariant();
		}

		/// <summary>
		/// Returns whether or not a parameter is specified in the list
		/// Returns true as long as the parameter is specified, it does not have to have a corresponding value.
		/// </summary>
		/// <param name="parameterStr">The full parameter string.</param>
		/// <param name="parameterName">the parameter for which to return a value from.</param>
		/// <returns>true if the parameter is specified, else false.</returns>
		public static bool HasParam(string parameterStr, string parameterName)
		{
			parameterName = NormalizeParameterName(parameterName);

			if (string.IsNullOrWhiteSpace(parameterStr))
			{
				return false;
			}

			var parameters = parameterStr
				.SplitWithEmpty("|")
				.Select(s => s.Trim())
				.Select(s => s.SplitWithEmpty("=").FirstOrDefault())
				.Where(s => !string.IsNullOrWhiteSpace(s))
				.Select(s => s!.Trim())
				.Where(s => !string.IsNullOrWhiteSpace(s));

			return parameters.Any(s => string.Equals(s, parameterName, StringComparison.OrdinalIgnoreCase));
		}

		/// <summary>
		/// Returns the value of the given parameter from the parameter list in string form.
		/// </summary>
		/// <param name="parameterStr">The full parameter string.</param>
		/// <param name="paramName">the parameter for which to return a value from.</param>
		/// <returns>The value of the given parameter if it is exists, else empty string.</returns>
		public static string GetValueFor(string parameterStr, string parameterName)
		{
			parameterName = NormalizeParameterName(parameterName);

			if (string.IsNullOrWhiteSpace(parameterStr))
			{
				return "";
			}

			var args = parameterStr
				.SplitWithEmpty("|")
				.Select(s => s.Trim());
			foreach (var arg in args)
			{
				var pair = arg
					.SplitWithEmpty("=")
					.Select(s => s.Trim())
					.ToList();
				if (pair.Count > 1 && string.Equals(pair[0], parameterName, StringComparison.OrdinalIgnoreCase))
				{
					return string.IsNullOrWhiteSpace(pair[1])
						? ""
						: pair[1];
				}
			}

			return "";
		}

		/// <summary>
		/// Takes the given string and parses it to an int if possible.
		/// If an integer can not ber parsed from the given value, null is returned.
		/// </summary>
		public static int? GetInt(string parameterStr, string parameterName)
		{
			var val = GetValueFor(parameterStr, parameterName).ToLower();

			var result = int.TryParse(val, out int parsedVal);
			if (result)
			{
				return parsedVal;
			}

			return null;
		}

		/// <summary>
		/// Takes the given string and parses it to an int if possible.
		/// But also accepts Y prefixed values such as Y2014.
		/// </summary>
		public static int? GetYear(string parameterStr, string parameterName)
		{
			var val = GetValueFor(parameterStr, parameterName).ToLower();

			if (string.IsNullOrWhiteSpace(val))
			{
				return null;
			}

			val = val.TrimStart('y');

			var result = int.TryParse(val, out int parsedVal);
			if (result)
			{
				return parsedVal;
			}

			return null;
		}

		public static IEnumerable<int> GetInts(string parameterStr, string parameterName)
		{
			if (string.IsNullOrWhiteSpace(parameterStr))
			{
				return Enumerable.Empty<int>();
			}

			return GetValueFor(parameterStr, parameterName).CsvToInts();
		}
	}

	internal static class ExtensionsToJunkLater
	{
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

		/// <summary>
		/// Takes a comma separated string and returns a list of values.
		/// </summary>
		public static IEnumerable<string> CsvToStrings(this string? param)
		{
			// TODO: Rename this; this isn't "CSV".
			return string.IsNullOrWhiteSpace(param)
				? Enumerable.Empty<string>()
				: param
					.SplitWithEmpty(",")
					.Where(p => !string.IsNullOrWhiteSpace(p))
					.Select(p => p.Trim());
		}
	}
}
