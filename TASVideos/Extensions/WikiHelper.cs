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
				return homepage == userName
					&& userPermissions.Contains(PermissionTo.EditHomePage);
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
	}
}
