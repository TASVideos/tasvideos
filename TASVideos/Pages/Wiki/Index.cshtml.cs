using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

using TASVideos.Data.Entity;
using TASVideos.Extensions;
using TASVideos.Razor;
using TASVideos.Services;
using TASVideos.Tasks;

namespace TASVideos.Pages.Wiki
{
	[AllowAnonymous]
	public class IndexModel : BasePageModel
	{
		private readonly IWikiPages _wikiPages;
		private readonly WikiMarkupFileProvider _wikiMarkupFileProvider;

		public IndexModel(
			IWikiPages wikiPages,
			WikiMarkupFileProvider wikiMarkupFileProvider,
			UserTasks userTasks)
			: base(userTasks)
		{
			_wikiPages = wikiPages;
			_wikiMarkupFileProvider = wikiMarkupFileProvider;
		}

		public WikiPage WikiPageData { get; set; }
		public string RazorPageName { get; set; }
		public IActionResult OnGet(string url, int? revision = null)
		{
			url = url.Trim('/').Replace(".html", "");

			if (url.ToLower() == "frontpage")
			{
				return Redirect("/");
			}

			if (!WikiHelper.IsValidWikiPageName(url))
			{
				return RedirectToPage("/Wiki/PageNotFound", new { possibleUrl = WikiHelper.NormalizeWikiPageName(url) });
			}

			var existingPage = _wikiPages.Page(url, revision);

			if (existingPage != null)
			{
				ViewData["WikiPage"] = existingPage;
				ViewData["Title"] = existingPage.PageName;
				ViewData["Layout"] = "_WikiLayout";
				_wikiMarkupFileProvider.WikiPages = _wikiPages;
				
				WikiPageData = existingPage;
				RazorPageName = WikiMarkupFileProvider.Prefix + existingPage.Id;
				return Page();
			}

			// Account for garbage revision values
			if (revision.HasValue && _wikiPages.Exists(url)) 
			{
				return Redirect("/" + url);
			}

			return RedirectToPage("/Wiki/PageDoesNotExist", new { url });
		}
	}
}
