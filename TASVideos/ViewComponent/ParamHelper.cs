using System;
using System.Linq;

namespace TASVideos.ViewComponents
{
	// ***************
	// Parameter helpers
	// These helpers assist with parsing wiki markup parameters from the user
	// By design they need to gracefully handle all sorts of nonsense input from the user
	// ***************
	public static class ParamHelper
	{
		/// <summary>
		/// Returns whether or not a parameter is specified in the list
		/// Returns true as long as the parameter is specified, it does not have to have a corresponding value
		/// </summary>
		/// <param name="parameterStr">The full parameter string</param>
		/// <param name="parameterName">the parameter for which to return a value from</param>
		/// <returns>true if the parameter is specified, else false</returns>
		public static bool HasParam(string parameterStr, string parameterName)
		{
			if (string.IsNullOrWhiteSpace(parameterStr))
			{
				return false;
			}

			if (string.IsNullOrWhiteSpace(parameterName))
			{
				return false;
			}
			
			var parameters = parameterStr
				.Split(new[] { "|" }, StringSplitOptions.RemoveEmptyEntries)
				.Select(s => s.Trim())
				.Select(s => s.Split(new[] { "=" }, StringSplitOptions.RemoveEmptyEntries).FirstOrDefault())
				.Where(s => !string.IsNullOrWhiteSpace(s))
				.Select(s => s.Trim())
				.Where(s => !string.IsNullOrWhiteSpace(s));

			return parameters.Any(s => string.Equals(s, parameterName, StringComparison.OrdinalIgnoreCase));
		}

		/// <summary>
		/// Returns the value of the given parameter from the parameter list in string form
		/// </summary>
		/// <param name="parameterStr">The full parameter string</param>
		/// <param name="paramName">the parameter for which to return a value from</param>
		/// <returns>The value of the given parameter if it is exists, else empty string</returns>
		public static string GetValueFor(string parameterStr, string paramName)
		{
			if (string.IsNullOrWhiteSpace(parameterStr))
			{
				return "";
			}

			if (string.IsNullOrWhiteSpace(paramName))
			{
				return "";
			}

			var args = parameterStr
				.Split(new[] { "|" }, StringSplitOptions.RemoveEmptyEntries)
				.Select(s => s.Trim());
			foreach (var arg in args)
			{
				var pair = arg
					.Split(new[] { "=" }, StringSplitOptions.RemoveEmptyEntries)
					.Select(s => s.Trim())
					.ToList();
				if (pair.Count > 1 && string.Equals(pair[0], paramName, StringComparison.OrdinalIgnoreCase))
				{
					if (string.IsNullOrWhiteSpace(pair[1]))
					{
						return "";
					}

					return pair[1];
				}
			}

			return "";
		}

		/// <summary>
		/// Takes the given string value and parses it to a bool if possible
		/// If a true/false value can not be determined, null is returned
		/// Possible values (case insensitive): true/false, yes/no, y/n
		/// if a string is null, empty, or whitespace, null is returned
		/// </summary>
		public static bool? GetBool(string parameterStr, string param)
		{
			if (string.IsNullOrWhiteSpace(param))
			{
				return null;
			}

			string val = GetValueFor(parameterStr, param)?.ToLower();
			if (val == "true"
				|| val == "yes"
				|| val == "y"
				|| val == "1")
			{
				return true;
			}

			if (val == "false"
				|| val == "no"
				|| val == "n"
				|| val == "0")
			{
				return false;
			}

			return null;
		}

		/// <summary>
		/// Takes the given string and parses it to an int if possible
		/// If an integer can not ber parsed from the given value, null is returned
		/// </summary>
		public static int? GetInt(string parameterStr, string param)
		{
			if (string.IsNullOrWhiteSpace(param))
			{
				return null;
			}

			var val = GetValueFor(parameterStr, param)?.ToLower();

			var result = int.TryParse(val, out int parsedVal);
			if (result)
			{
				return parsedVal;
			}

			return null;
		}
	}
}
