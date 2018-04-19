using System.Linq;
using Microsoft.AspNetCore.Mvc;

namespace TASVideos.ViewComponents
{
	public class ModuleComponentBase : ViewComponent
	{
		// ***************
		// Parameter helpers
		// These helpers assist with parsing wiki markup parameters from the user
		// By design they need to gracefully handle all sorts of nonsense input from the user
		// ***************

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

			return parameterStr
				.Split('|')
				.Where(s => !string.IsNullOrWhiteSpace(s))
				.Any(s => s == parameterName || s.StartsWith(parameterName + "="));
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

			var lowerParam = paramName.ToLower();

			var args = parameterStr.Split('|');
			foreach (var arg in args.Where(a => !string.IsNullOrWhiteSpace(a)))
			{
				var pair = arg.Split('=');
				if (pair.Length > 1 && pair[0]?.ToLower() == lowerParam)
				{
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
				|| val == "y")
			{
				return true;
			}

			if (val == "false"
				|| val == "no"
				|| val == "n")
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
