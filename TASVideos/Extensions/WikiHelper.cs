using System.Text.RegularExpressions;

using TASVideos.Data.Entity;

namespace TASVideos.Extensions;

// ReSharper disable PossibleMultipleEnumeration
public static class WikiHelper
{
	private const string HomePagesPrefix = "HomePages/";
	public static bool UserCanEditWikiPage(string? pageName, string? userName, IEnumerable<PermissionTo> userPermissions)
	{
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
				|| userPermissions.Contains(PermissionTo.EditSystemPages)
				|| userPermissions.Contains(PermissionTo.EditSubmissions)
				|| userPermissions.Contains(PermissionTo.EditPublicationMetaData);
		}

		if (IsPublicationPage(pageName).HasValue)
		{
			return userPermissions.Contains(PermissionTo.EditPublicationMetaData);
		}

		if (IsSubmissionPage(pageName).HasValue)
		{
			return userPermissions.Contains(PermissionTo.EditSubmissions);
		}

		if (pageName.StartsWith("GameResources/"))
		{
			return userPermissions.Contains(PermissionTo.EditGameResources);
		}

		if (pageName.StartsWith("System/"))
		{
			return userPermissions.Contains(PermissionTo.EditSystemPages);
		}

		if (pageName.StartsWith(HomePagesPrefix))
		{
			// A home page is defined as Homepages/[UserName]
			// If a user can exploit this fact to create an exploit
			// then we should first reconsider rules about allowed patterns of usernames and what defines a valid wiki page
			// before deciding to nuke this feature
			var homepage = pageName[HomePagesPrefix.Length..].Split('/')[0];
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
			test = pageName.Replace(HomePagesPrefix, "");
			var slashIndex = test.IndexOf('/') + 1;
			if (slashIndex == 0)
			{
				return true; // Just HomePage/[username] so it is automatically valid
			}

			test = test[slashIndex..];
		}

		return !string.IsNullOrWhiteSpace(test)
			&& !test.StartsWith('/')
			&& !test.EndsWith('/')
			&& !test.EndsWith(".html")
			&& Regex.IsMatch(test, @"^\S*$")
			&& char.IsUpper(test[0])
			&& IsProperCased(test);
	}

	public static bool IsSystemGameResourcePath(this string path)
	{
		if (string.IsNullOrWhiteSpace(path) || string.IsNullOrWhiteSpace(path.Trim('/')))
		{
			return false;
		}

		path = path
			.Trim('/')
			.Replace("GameResources", "");

		return path.SplitWithEmpty("/").Length == 1;
	}

	public static string SystemGameResourcePath(this string path)
	{
		if (!path.IsSystemGameResourcePath())
		{
			return "";
		}

		return path
			.Trim('/')
			.Replace("GameResources", "")
			.SplitWithEmpty("/")
			.First();
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

	public static bool IsHomePage(string? pageName)
	{
		return !string.IsNullOrWhiteSpace(pageName)
			&& pageName.StartsWith(HomePagesPrefix);
	}

	public static string ToUserName(string pageName)
	{
		if (!IsHomePage(pageName))
		{
			return "";
		}

		return pageName
			.Trim('/')
			.Split('/')
			.Skip(1)
			.First();
	}

	public static bool IsSystemPage(string? pageName)
	{
		return !string.IsNullOrWhiteSpace(pageName)
			&& pageName.StartsWith("System/");
	}

	public static bool IsGameResourcesPage(string? pageName)
	{
		return !string.IsNullOrWhiteSpace(pageName)
			&& pageName.StartsWith("GameResources/");
	}

	public static int? IsPublicationPage(string? pageName)
	{
		if (string.IsNullOrWhiteSpace(pageName))
		{
			return null;
		}

		if (pageName.StartsWith("InternalSystem/PublicationContent/M"))
		{
			var result = int.TryParse(
				pageName.Replace("InternalSystem/PublicationContent/M", ""), out int id);

			if (result)
			{
				return id;
			}
		}

		return null;
	}

	public static int? IsSubmissionPage(string? pageName)
	{
		if (string.IsNullOrWhiteSpace(pageName))
		{
			return null;
		}

		if (pageName.StartsWith("InternalSystem/SubmissionContent/S"))
		{
			var result = int.TryParse(
				pageName.Replace("InternalSystem/SubmissionContent/S", ""), out int id);

			if (result)
			{
				return id;
			}
		}

		return null;
	}

	/// <summary>
	/// Fixes Internal system page links to be their public counter parts ex: InternalSystem/SubmissionContent/S4084 to 4084S.
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
		return !string.IsNullOrWhiteSpace(link)
			&& link.StartsWith(LinkConstants.SubmissionWikiPage);
	}

	private static bool IsInternalPublicationLink(string link)
	{
		return !string.IsNullOrWhiteSpace(link)
			&& link.StartsWith(LinkConstants.PublicationWikiPage);
	}

	private static bool IsInternalGameLink(string link)
	{
		return !string.IsNullOrWhiteSpace(link)
			&& link.StartsWith(LinkConstants.GameWikiPage);
	}

	private static string FixInternalSubmissionLink(string link)
	{
		return FixInternalLink(link, LinkConstants.SubmissionWikiPage, "S");
	}

	private static string FixInternalPublicationLink(string link)
	{
		return FixInternalLink(link, LinkConstants.PublicationWikiPage, "M");
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

		var paths = pageName.SplitWithEmpty("/");

		// Must begin with a capital letter, or a number
		return paths.All(p => char.IsUpper(p[0]) || char.IsNumber(p[0]));
	}
}
