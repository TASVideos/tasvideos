using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

using TASVideos.Data.Entity;
using TASVideos.Extensions;
using TASVideos.Services;

namespace TASVideos.Pages.Wiki
{
	[AllowAnonymous]
	public class IndexModel : BasePageModel
	{
		private readonly IWikiPages _wikiPages;

		public IndexModel(IWikiPages wikiPages)
		{
			_wikiPages = wikiPages;
		}

		public string Markup { get; set; }

		public WikiPage WikiPage { get; set; }

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

			WikiPage = _wikiPages.Page(url, revision);

			if (WikiPage != null)
			{
				ViewData["WikiPage"] = WikiPage;
				ViewData["Title"] = WikiPage.PageName;
				Markup = WikiPage.Markup;
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
