using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

using TASVideos.Data.Entity;

namespace TASVideos.Extensions
{
	public static class WikiHelper
	{
		public static bool UserCanEditWikiPage(string pageName, string userName, IEnumerable<PermissionTo> userPermissions)
		{
			if (userPermissions == null)
			{
				throw new ArgumentNullException($"{nameof(userPermissions)} can not be null");
			}

			if (string.IsNullOrWhiteSpace(pageName) || string.IsNullOrWhiteSpace(userName))
			{
				return false;
			}

			pageName = pageName.Trim('/');

			if (pageName.StartsWith("GameResources/"))
			{
				return userPermissions.Contains(PermissionTo.EditGameResources);
			}

			if (pageName.StartsWith("System/"))
			{
				return userPermissions.Contains(PermissionTo.EditSystemPages);
			}

			if (pageName.StartsWith("Homepages/"))
			{
				// A home page is defiend as Homepages/[UserName]
				// If a user can exploit this fact to create an exploit
				// then we should first reconsider rules about allowed patterns of usernames and what defines a valid wiki page
				// before deciding to nuke this feature
				var homepage = pageName.Split("Homepages/")[1];
				if (string.Equals(homepage, userName, StringComparison.OrdinalIgnoreCase)
					&& userPermissions.Contains(PermissionTo.EditHomePage))
				{
					return true;
				}

				// Notice we fall back to EditWikiPages if it is not the user's homepage, regular editors should be able to edit homepages
			}

			return userPermissions.Contains(PermissionTo.EditWikiPages);
		}

		public static bool IsValidWikiPageName(string pageName)
		{
			return !string.IsNullOrWhiteSpace(pageName)
				&& !pageName.StartsWith('/')
				&& !pageName.EndsWith('/')
				&& Regex.IsMatch(pageName, @"^\S*$")
				&& char.IsUpper(pageName[0])
				&& !pageName.EndsWith(".html")
				&& IsProperCased(pageName);
		}

		public static string TryConvertToValidPageName(string pageName)
		{
			if (string.IsNullOrWhiteSpace(pageName))
			{
				return "";
			}

			pageName = Regex.Replace(pageName
				.Replace(".html", "")
				.Trim('/'), @"\s", "");

			return ConvertProperCase(pageName);
		}

		// Does not check for null that should have already been done
		// Slashes must have already been trimmed or it will break
		private static bool IsProperCased(string pageName)
		{
			
			if (!char.IsUpper(pageName[0]))
			{
				return false;
			}

			var slashes = AllIndexesOf(pageName, "/");
			foreach (var slash in slashes)
			{
				if (!char.IsUpper(pageName[slash + 1]))
				{
					return false;
				}
			}

			return true;
		}

		private static string ConvertProperCase(string pageName)
		{
			pageName = char.ToUpper(pageName[0]) + pageName.Substring(1);

			var slashes = AllIndexesOf(pageName, "/");
			foreach (var slash in slashes)
			{
				pageName = pageName.Substring(0, slash + 1)
					+ char.ToUpper(pageName[slash + 1])
					+ pageName.Substring(slash + 2);
			}

			return pageName;
		}

		private static IEnumerable<int> AllIndexesOf(string str, string searchstring)
		{
			int minIndex = str.IndexOf(searchstring);
			while (minIndex != -1)
			{
				yield return minIndex;
				minIndex = str.IndexOf(searchstring, minIndex + searchstring.Length);
			}
		}

		// ***************
		// Parameter helpers
		// These helpers assist with parsing wiki markup parameters from the user
		// By design they need to gracefully handle all sorts of nonsense input from the user
		// ***************

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
		public static bool? GetBool(string val)
		{
			if (string.IsNullOrWhiteSpace(val))
			{
				return null;
			}

			string lowerVal = val.ToLower();
			if (lowerVal == "true"
				|| lowerVal == "yes"
				|| lowerVal == "y")
			{
				return true;
			}

			if (lowerVal == "false"
				|| lowerVal == "no"
				|| lowerVal == "n")
			{
				return false;
			}

			return null;
		}

		/// <summary>
		/// Takes the given string and parses it to an int if possible
		/// If an integer can not ber parsed from the given value, null is returned
		/// </summary>
		public static int? GetInt(string val)
		{
			var result = int.TryParse(val, out int parsedVal);
			if (result)
			{
				return parsedVal;
			}

			return null;
		}
	}
}
