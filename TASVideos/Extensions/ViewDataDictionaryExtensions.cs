using Microsoft.AspNetCore.Mvc.ViewFeatures;
using TASVideos.Core.Services.Wiki;
using TASVideos.Pages;

namespace TASVideos.Extensions;

public static class ViewDataDictionaryExtensions
{
	public static string UniqueId(this ViewDataDictionary viewData)
	{
		return "_" + Guid.NewGuid().ToString().Replace("-", "").ToLower();
	}

	public static void SetMetaTags(this ViewDataDictionary viewData, MetaTag metaTags)
	{
		viewData["MetaTags"] = metaTags;
	}

	public static MetaTag GetMetaTags(this ViewDataDictionary viewData)
		=> viewData["MetaTags"] as MetaTag ?? MetaTag.Default;

	public static void SetTitle(this ViewDataDictionary viewData, string title)
	{
		viewData["Title"] = title;
	}

	public static void SetHeading(this ViewDataDictionary viewData, string heading)
	{
		viewData["Heading"] = heading;
	}

	public static string GetHeading(this ViewDataDictionary viewData)
	{
		var title = viewData["Title"]?.ToString();
		if (viewData.GetWikiPage() is not null)
		{
			title = title.SplitPathCamelCase();
		}

		return viewData["Heading"]?.ToString() ?? title ?? "";
	}

	public static IWikiPage? GetWikiPage(this ViewDataDictionary viewData)
	{
		return viewData["WikiPage"] as IWikiPage;
	}

	public static void SetWikiPage(this ViewDataDictionary viewData, IWikiPage wikiPage)
	{
		viewData["WikiPage"] = wikiPage;
	}

	public static void SetNavigation(this ViewDataDictionary viewData, int id, string suffix)
	{
		viewData["NavigationId"] = id;
		viewData["NavigationSuffix"] = suffix;
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
