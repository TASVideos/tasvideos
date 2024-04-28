using Microsoft.AspNetCore.Mvc.ViewFeatures;
using TASVideos.Core.Services.Wiki;
using TASVideos.Pages;

namespace TASVideos.Extensions;

public static class ViewDataDictionaryExtensions
{
	public static IReadOnlyCollection<PermissionTo> UserPermissions(this ViewDataDictionary viewData)
		=> viewData["UserPermissions"] as IReadOnlyCollection<PermissionTo> ?? [];

	public static string UniqueId(this ViewDataDictionary viewData)
	{
		return "_" + Guid.NewGuid().ToString().Replace("-", "").ToLower();
	}

	public static bool UserHas(this ViewDataDictionary viewData, PermissionTo permission)
	{
		return viewData.UserPermissions().Contains(permission);
	}

	public static bool UserHasAny(this ViewDataDictionary viewData, IEnumerable<PermissionTo> permissions)
	{
		var userPerm = viewData.UserPermissions();
		return permissions.Any(permission => userPerm.Contains(permission));
	}

	public static void SetMetaTags(this ViewDataDictionary viewData, MetaTag metaTags)
	{
		viewData["MetaTags"] = metaTags;
	}

	public static void SetTitle(this ViewDataDictionary viewData, string title)
	{
		viewData["Title"] = title;
	}

	public static void SetHeading(this ViewDataDictionary viewData, string heading)
	{
		viewData["Heading"] = heading;
	}

	public static IWikiPage? GetWikiPage(this ViewDataDictionary viewData)
	{
		return viewData["WikiPage"] as IWikiPage;
	}

	public static void SetWikiPage(this ViewDataDictionary viewData, IWikiPage wikiPage)
	{
		viewData["WikiPage"] = wikiPage;
	}

	public static int Int(this ViewDataDictionary viewData, string key)
	{
		var obj = viewData[key];
		if (obj is int i)
		{
			return i;
		}

		return 0;
	}
}
