﻿using System.Text.RegularExpressions;
using TASVideos.Core.Services.Wiki;

namespace TASVideos.Extensions;

public static class WikiHelper
{
	public static bool UserCanEditWikiPage(string? pageName, string? userName, IReadOnlyCollection<PermissionTo> userPermissions, out HashSet<PermissionTo> relevantPermissions)
	{
		relevantPermissions = [];

		if (string.IsNullOrWhiteSpace(pageName) || string.IsNullOrWhiteSpace(userName))
		{
			return false;
		}

		pageName = pageName.Trim('/');

		// Anyone who can edit anything (including the user's own homepage) should be allowed to edit his
		if (pageName == "SandBox")
		{
			relevantPermissions = [
				PermissionTo.EditGameResources,
				PermissionTo.EditHomePage,
				PermissionTo.EditWikiPages,
				PermissionTo.EditSystemPages,
				PermissionTo.EditSubmissions,
				PermissionTo.EditPublicationMetaData
			];
			return relevantPermissions.Any(userPermissions.Contains);
		}

		if (IsPublicationPage(pageName, out _))
		{
			relevantPermissions = [PermissionTo.EditPublicationMetaData];
			return relevantPermissions.Any(userPermissions.Contains);
		}

		if (IsSubmissionPage(pageName, out _))
		{
			relevantPermissions = [PermissionTo.EditSubmissions];
			return relevantPermissions.Any(userPermissions.Contains);
		}

		if (pageName.StartsWith("GameResources/"))
		{
			relevantPermissions = [PermissionTo.EditGameResources];
			return relevantPermissions.Any(userPermissions.Contains);
		}

		if (pageName.StartsWith("System/"))
		{
			relevantPermissions = [PermissionTo.EditSystemPages];
			return relevantPermissions.Any(userPermissions.Contains);
		}

		if (pageName.StartsWith(LinkConstants.HomePages))
		{
			relevantPermissions = [PermissionTo.EditHomePage];

			// A home page is defined as Homepages/[UserName]
			// If a user can exploit this fact to create an exploit
			// then we should first reconsider rules about allowed patterns of usernames and what defines a valid wiki page
			// before deciding to nuke this feature
			var homepage = pageName[LinkConstants.HomePages.Length..].Split('/')[0];
			if (string.Equals(homepage, userName, StringComparison.OrdinalIgnoreCase)
				&& relevantPermissions.Any(userPermissions.Contains))
			{
				return true;
			}

			// Notice we fall back to EditWikiPages if it is not the user's homepage, regular editors should be able to edit homepages
		}

		relevantPermissions = [PermissionTo.EditWikiPages];
		return relevantPermissions.Any(userPermissions.Contains);
	}

	public static bool IsValidWikiPageName(string pageName, bool validateLoosely = false)
	{
		// If the page is a homepage, then don't validate the username portion
		// However we want to validate any subpages off the user
		// Ex:
		// HomePages/My Bad UserName = valid
		// HomePages/My Bad UserName/My Bad Subpage = invalid
		string test = pageName;
		if (IsHomePage(pageName))
		{
			test = pageName.Replace(LinkConstants.HomePages, "");
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
			&& (validateLoosely || (
				char.IsUpper(test[0])
				&& IsProperCased(test)));
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

	public static bool IsHomePage(this IWikiPage page)
	{
		return IsHomePage(page.PageName);
	}

	public static bool IsSystemPage(this IWikiPage page)
	{
		return IsSystemPage(page.PageName);
	}

	public static bool IsGameResourcesPage(this IWikiPage page)
	{
		return IsGameResourcesPage(page.PageName);
	}

	public static bool IsHomePage(string? pageName)
	{
		return !string.IsNullOrEmpty(pageName)
			&& pageName.StartsWith(LinkConstants.HomePages)
			&& pageName.Length > LinkConstants.HomePages.Length;
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

	public static string EscapeUserName(string pageName)
	{
		if (!IsHomePage(pageName))
		{
			return pageName;
		}

		string[] splitPage = pageName.Trim('/').Split('/');
		if (splitPage.Length >= 2)
		{
			splitPage[1] = Uri.EscapeDataString(splitPage[1]);
		}

		return string.Join('/', splitPage);
	}

	public static bool IsSystemPage(string? pageName)
	{
		return !string.IsNullOrWhiteSpace(pageName)
			&& pageName.StartsWith("System/")
			&& pageName.Length > "System/".Length;
	}

	public static bool IsGameResourcesPage(string? pageName)
	{
		return !string.IsNullOrWhiteSpace(pageName)
			&& pageName.StartsWith("GameResources/")
			&& pageName.Length > "GameResources/".Length;
	}

	public static bool IsPublicationPage(string? pageName, out int id)
	{
		if (!string.IsNullOrWhiteSpace(pageName) && pageName.StartsWith(LinkConstants.PublicationWikiPage))
		{
			return int.TryParse(pageName[LinkConstants.PublicationWikiPage.Length..], out id);
		}

		id = 0;
		return false;
	}

	public static bool IsSubmissionPage(string? pageName, out int id)
	{
		if (!string.IsNullOrWhiteSpace(pageName) && pageName.StartsWith(LinkConstants.SubmissionWikiPage))
		{
			return int.TryParse(pageName[LinkConstants.SubmissionWikiPage.Length..], out id);
		}

		id = 0;
		return false;
	}

	/// <summary>
	/// Fixes Internal system page links to be their public counterparts ex: InternalSystem/SubmissionContent/S4084 to 4084S.
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

	public static string ToPublicationWikiPageName(int publicationId)
		=> $"{LinkConstants.PublicationWikiPage}{publicationId}";

	public static string ToSubmissionWikiPageName(int submissionId)
		=> $"{LinkConstants.SubmissionWikiPage}{submissionId}";

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
