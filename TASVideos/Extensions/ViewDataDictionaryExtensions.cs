using Microsoft.AspNetCore.Mvc.ViewFeatures;
using TASVideos.Core.Services.Wiki;
using TASVideos.Pages;

namespace TASVideos.Extensions;

public static class ViewDataDictionaryExtensions
{
	extension(ViewDataDictionary viewData)
	{
		public void IgnorePageTitle() => viewData["IgnorePageTitle"] = true;
		public bool UsePageTitle() => viewData["IgnorePageTitle"] is not true;
		public string UniqueId() => "_" + Guid.NewGuid().ToString().Replace("-", "").ToLower();
		public void SetMetaTags(MetaTag metaTags) => viewData["MetaTags"] = metaTags;
		public MetaTag GetMetaTags() => viewData["MetaTags"] as MetaTag ?? MetaTag.Default;
		public string? GetTitle() => viewData["Title"]?.ToString();
		public void SetTitle(string title) => viewData["Title"] = title;
		public void SetCanonicalUrl(string canonicalUrl) => viewData["CanonicalUrl"] = canonicalUrl;
		public string? GetCanonicalUrl() => viewData["CanonicalUrl"]?.ToString();
		public string GetFavicon() => viewData["Favicon"]?.ToString() ?? "favicon.ico";
		public void UseGreenFavicon() => viewData["Favicon"] = "favicon_green.ico";
		public void UseRedFavicon() => viewData["Favicon"] = "favicon_red.ico";
		public void SetHeading(string heading) => viewData["Heading"] = heading;

		public string GetHeading()
		{
			var title = viewData.GetTitle();
			if (viewData.GetWikiPage() is not null)
			{
				title = title.SplitPathCamelCase();
			}

			return viewData["Heading"]?.ToString() ?? title ?? "";
		}

		public IWikiPage? GetWikiPage() => viewData["WikiPage"] as IWikiPage;

		public void SetWikiPage(IWikiPage wikiPage) => viewData["WikiPage"] = wikiPage;

		public void SetNavigation(int id, string fmtStr = "{0}")
		{
			viewData["NavigationId"] = id;
			viewData["NavigationFmtStr"] = fmtStr;
		}

		public string GetNavigationFormatStr() => viewData["NavigationFmtStr"] as string ?? "";

		public string ActivePageClass(string page)
		{
			var activePage = viewData["ActivePage"] as string;
			return string.Equals(activePage, page, StringComparison.OrdinalIgnoreCase) ? "active" : "";
		}

		public void AddActivePage(string activePage) => viewData["ActivePage"] = activePage;

		public int? Int(string key)
		{
			var obj = viewData[key];
			return obj is int i ? i : null;
		}

		public void EnableClientSideValidation() => viewData["client-side-validation"] = true;
		public bool ClientSideValidationEnabled() => viewData["client-side-validation"] is not null;
		public void UseSelectImprover() => viewData["use-select-improver"] = true;
		public bool UsesSelectImprover() => viewData["use-select-improver"] is not null;
		public void UseUserSearch() => viewData["use-user-search"] = true;
		public bool UsesUserSearch() => viewData["use-user-search"] is not null;
		public void UseBackupText() => viewData["use-backup-text"] = true;
		public bool UsesBackupText() => viewData["use-backup-text"] is not null;
		public void UseStringList() => viewData["use-string-list"] = true;
		public bool UsesStringList() => viewData["use-string-list"] is not null;
		public void UsePreview() => viewData["use-preview"] = true;
		public bool UsesPreview() => viewData["use-preview"] is not null;
		public void UseShowMore() => viewData["use-show-more"] = true;
		public bool UsesShowMore() => viewData["use-show-more"] is not null;
		public void UsePostHelper() => viewData["use-post-helper"] = true;
		public bool UsesPostHelper() => viewData["use-post-helper"] is not null;
		public void UseWikiEditHelper() => viewData["use-wiki-edit-helper"] = true;
		public bool UsesWikiEditHelper() => viewData["use-wiki-edit-helper"] is not null;
		public void UseDiff() => viewData["use-diff"] = true;
		public bool UsesDiff() => viewData["use-diff"] is not null;
		public void UseMoodPreview() => viewData["use-mood-preview"] = true;
		public bool UsesMoodPreview() => viewData["use-mood-preview"] is not null;
		public void UseClientFileCompression() => viewData["use-client-file-compression"] = true;
		public bool UsesClientFileCompression() => viewData["use-client-file-compression"] is not null;
	}
}
