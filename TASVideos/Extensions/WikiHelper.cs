using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;

using TASVideos.Data.Constants;
using TASVideos.Data.Entity;

namespace TASVideos.Extensions
{
	// ReSharper disable PossibleMultipleEnumeration
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

			// Anyone who can edit anything (including the user's own homepage) should be allowed to edit his
			if (pageName == "SandBox")
			{
				return userPermissions.Contains(PermissionTo.EditGameResources)
					|| userPermissions.Contains(PermissionTo.EditHomePage)
					|| userPermissions.Contains(PermissionTo.EditWikiPages)
					|| userPermissions.Contains(PermissionTo.EditSystemPages);
			}

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
				// A home page is defined as Homepages/[UserName]
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
			// If the page is a homepage, then don't validate the username portion
			// However we want to validate any subpages off the user
			// Ex:
			// HomePages/My Bad UserName = valid
			// HomePages/My Bad UserName/My Bad Subpage = invalid
			string test = pageName;
			if (IsHomePage(pageName))
			{
				test = pageName.Replace("HomePages/", "");
				var slashIndex = test.IndexOf('/') + 1;
				if (slashIndex == 0)
				{
					return true; // Just HomePage/[username] so it is automatically valid
				}

				test = test.Substring(slashIndex, test.Length - slashIndex);
			}

			return !string.IsNullOrWhiteSpace(test)
				&& !test.StartsWith('/')
				&& !test.EndsWith('/')
				&& !test.EndsWith(".html")
				&& Regex.IsMatch(test, @"^\S*$")
				&& char.IsUpper(test[0])
				&& IsProperCased(test);
		}

		public static bool IsHomePage(this WikiPage page)
		{
			return IsHomePage(page.PageName);
		}

		public static bool IsSystemPage(this WikiPage page)
		{
			return IsSystemPage(page.PageName);
		}

		public static bool IsGameResourcesPage(this WikiPage page)
		{
			return IsGameResourcesPage(page.PageName);
		}

		public static bool IsHomePage(string pageName)
		{
			return pageName.StartsWith("HomePages/");
		}

		public static bool IsSystemPage(string pageName)
		{
			return pageName.StartsWith("System/");
		}

		public static bool IsGameResourcesPage(string pageName)
		{
			return pageName.StartsWith("GameResources/");
		}

		/// <summary>
		/// Fixes Internal system page links to be their public counter parts ex: InternalSystem/SubmissionContent/S4084 to 4084S
		/// </summary>
		public static string ProcessLink(string link)
		{
			if (IsInternalSubmissionLink(link))
			{
				return FixInternalSubmissionLink(link);
			}

			if (IsInternalPublicationLink(link))
			{
				return FixInternalPublicationLink(link);
			}

			if (IsInternalGameLink(link))
			{
				return FixInternalGameLink(link);
			}

			return link;
		}

		private static bool IsInternalSubmissionLink(string link)
		{
			return link.StartsWith(LinkConstants.SubmissionWikiPage);
		}

		private static bool IsInternalPublicationLink(string link)
		{
			return link.StartsWith(LinkConstants.PublicationWikiPage);
		}

		private static bool IsInternalGameLink(string link)
		{
			return link.StartsWith(LinkConstants.GameWikiPage);
		}

		private static string FixInternalSubmissionLink(string link)
		{
			return FixInternalLink(link, LinkConstants.SubmissionWikiPage, "S");
		}

		private static string FixInternalPublicationLink(string link)
		{
			return FixInternalLink(link, LinkConstants.PublicationWikiPage, "P");
		}

		private static string FixInternalGameLink(string link)
		{
			return FixInternalLink(link, LinkConstants.GameWikiPage, "G");
		}

		private static string FixInternalLink(string link, string internalPrefix, string suffix)
		{
			var result = int.TryParse(link.Replace(internalPrefix, ""), out int id);
			if (result)
			{
				return id + suffix;
			}

			return "";
		}

		private static bool IsProperCased(string pageName)
		{
			if (string.IsNullOrWhiteSpace(pageName))
			{
				return false;
			}

			var paths = pageName.Split(new[] { "/" }, StringSplitOptions.RemoveEmptyEntries);

			// Must begin with a capital letter, with one exception, if the path is a year. But only years between 2000-2099 for now. This is to support awards pages: Awards/2007, Awards/2008 etc
			return paths.All(p => char.IsUpper(p[0]) || (p.Length == 4 && p.StartsWith("20")));
		}

		public static string NormalizeWikiPageName(string link)
		{
			if (link.StartsWith("user:"))
			{
				link = "HomePages/" + link.Substring(5);
			}
			else
			{
				// Support links like [Judge Guidelines] linking to [JudgeGuidelines]
				// We don't do this replacement if link is a user module in order to support users with spaces such as Walker Boh
				link = link.Replace(" ", "");
			}

			if (link.EndsWith(".html", true, CultureInfo.InvariantCulture))
			{
				link = link.Substring(0, link.Length - 5);
			}

			link = link.Trim('/');
			link = string.Join("/", link.Split('/').Select(s => char.ToUpperInvariant(s[0]) + s.Substring(1)));
			return link;
		}
	}
}
