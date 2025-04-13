using Microsoft.AspNetCore.Mvc.ViewFeatures;
using TASVideos.Core.Services.Wiki;
using TASVideos.Pages;

namespace TASVideos.Extensions;

public static class ViewDataDictionaryExtensions
{
	public static void IgnorePageTitle(this ViewDataDictionary viewData)
		=> viewData["IgnorePageTitle"] = true;

	public static bool UsePageTitle(this ViewDataDictionary viewData) => viewData["IgnorePageTitle"] is not true;

	public static string UniqueId(this ViewDataDictionary viewData)
		=> "_" + Guid.NewGuid().ToString().Replace("-", "").ToLower();

	public static void SetMetaTags(this ViewDataDictionary viewData, MetaTag metaTags)
		=> viewData["MetaTags"] = metaTags;

	public static MetaTag GetMetaTags(this ViewDataDictionary viewData)
		=> viewData["MetaTags"] as MetaTag ?? MetaTag.Default;

	public static string? GetTitle(this ViewDataDictionary viewData) => viewData["Title"]?.ToString();

	public static void SetTitle(this ViewDataDictionary viewData, string title)
		=> viewData["Title"] = title;

	public static void SetCanonicalUrl(this ViewDataDictionary viewData, string canonicalUrl)
		=> viewData["CanonicalUrl"] = canonicalUrl;

	public static string? GetCanonicalUrl(this ViewDataDictionary viewData)
		=> viewData["CanonicalUrl"]?.ToString();

	public static string GetFavicon(this ViewDataDictionary viewData) => viewData["Favicon"]?.ToString() ?? "favicon.ico";

	public static void UseGreenFavicon(this ViewDataDictionary viewData)
		=> viewData["Favicon"] = "favicon_green.ico";

	public static void UseRedFavicon(this ViewDataDictionary viewData)
		=> viewData["Favicon"] = "favicon_red.ico";

	public static void SetHeading(this ViewDataDictionary viewData, string heading)
		=> viewData["Heading"] = heading;

	public static string GetHeading(this ViewDataDictionary viewData)
	{
		var title = viewData.GetTitle();
		if (viewData.GetWikiPage() is not null)
		{
			title = title.SplitPathCamelCase();
		}

		return viewData["Heading"]?.ToString() ?? title ?? "";
	}

	public static IWikiPage? GetWikiPage(this ViewDataDictionary viewData)
		=> viewData["WikiPage"] as IWikiPage;

	public static void SetWikiPage(this ViewDataDictionary viewData, IWikiPage wikiPage)
		=> viewData["WikiPage"] = wikiPage;

	public static void SetNavigation(this ViewDataDictionary viewData, int id, string fmtStr = "{0}")
	{
		viewData["NavigationId"] = id;
		viewData["NavigationFmtStr"] = fmtStr;
	}

	public static string GetNavigationFormatStr(this ViewDataDictionary viewData)
		=> viewData["NavigationFmtStr"] as string ?? "";

	public static string ActivePageClass(this ViewDataDictionary viewData, string page)
	{
		var activePage = viewData["ActivePage"] as string;
		return string.Equals(activePage, page, StringComparison.OrdinalIgnoreCase) ? "active" : "";
	}

	public static void AddActivePage(this ViewDataDictionary viewData, string activePage)
		=> viewData["ActivePage"] = activePage;

	public static int? Int(this ViewDataDictionary viewData, string key)
	{
		var obj = viewData[key];
		if (obj is int i)
		{
			return i;
		}

		return null;
	}

	public static void EnableClientSideValidation(this ViewDataDictionary viewData)
		=> viewData["client-side-validation"] = true;

	public static bool ClientSideValidationEnabled(this ViewDataDictionary viewData)
		=> viewData["client-side-validation"] is not null;

	public static void UseSelectImprover(this ViewDataDictionary viewData)
		=> viewData["use-select-improver"] = true;

	public static bool UsesSelectImprover(this ViewDataDictionary viewData)
		=> viewData["use-select-improver"] is not null;

	public static void UseUserSearch(this ViewDataDictionary viewData)
		=> viewData["use-user-search"] = true;

	public static bool UsesUserSearch(this ViewDataDictionary viewData)
		=> viewData["use-user-search"] is not null;

	public static void UseBackupText(this ViewDataDictionary viewData)
		=> viewData["use-backup-text"] = true;

	public static bool UsesBackupText(this ViewDataDictionary viewData)
		=> viewData["use-backup-text"] is not null;

	public static void UseStringList(this ViewDataDictionary viewData)
		=> viewData["use-string-list"] = true;

	public static bool UsesStringList(this ViewDataDictionary viewData)
		=> viewData["use-string-list"] is not null;

	public static void UsePreview(this ViewDataDictionary viewData)
		=> viewData["use-preview"] = true;

	public static bool UsesPreview(this ViewDataDictionary viewData)
		=> viewData["use-preview"] is not null;

	public static void UseShowMore(this ViewDataDictionary viewData)
		=> viewData["use-show-more"] = true;

	public static bool UsesShowMore(this ViewDataDictionary viewData)
		=> viewData["use-show-more"] is not null;

	public static void UsePostHelper(this ViewDataDictionary viewData)
		=> viewData["use-post-helper"] = true;

	public static bool UsesPostHelper(this ViewDataDictionary viewData)
		=> viewData["use-post-helper"] is not null;

	public static void UseWikiEditHelper(this ViewDataDictionary viewData)
		=> viewData["use-wiki-edit-helper"] = true;

	public static bool UsesWikiEditHelper(this ViewDataDictionary viewData)
		=> viewData["use-wiki-edit-helper"] is not null;

	public static void UseDiff(this ViewDataDictionary viewData)
		=> viewData["use-diff"] = true;

	public static bool UsesDiff(this ViewDataDictionary viewData)
		=> viewData["use-diff"] is not null;

	public static void UseMoodPreview(this ViewDataDictionary viewData)
		=> viewData["use-mood-preview"] = true;

	public static bool UsesMoodPreview(this ViewDataDictionary viewData)
		=> viewData["use-mood-preview"] is not null;

	public static void UseClientFileCompression(this ViewDataDictionary viewData)
		=> viewData["use-client-file-compression"] = true;

	public static bool UsesClientFileCompression(this ViewDataDictionary viewData)
		=> viewData["use-client-file-compression"] is not null;
}
