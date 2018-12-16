using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ViewComponents;

using TASVideos.Extensions;
using TASVideos.Razor;
using TASVideos.Services;

namespace TASVideos.ViewComponents
{
	public class RenderWikiPage : ViewComponent
	{
		private readonly IWikiPages _wikiPages;
		private readonly WikiMarkupFileProvider _wikiMarkupFileProvider;

		public RenderWikiPage(
			IWikiPages wikiPages,
			WikiMarkupFileProvider wikiMarkupFileProvider)
		{
			_wikiPages = wikiPages;
			_wikiMarkupFileProvider = wikiMarkupFileProvider;
		}

		public IViewComponentResult Invoke(string url, int? revision = null)
		{
			url = url.Trim('/');
			if (!WikiHelper.IsValidWikiPageName(url))
			{
				return new ContentViewComponentResult("");
			}

			var existingPage = _wikiPages.Page(url, revision);

			if (existingPage != null)
			{
				ViewData["WikiPage"] = existingPage;
				ViewData["Title"] = existingPage.PageName;
				ViewData["Layout"] = null;
				_wikiMarkupFileProvider.WikiPages = _wikiPages;
				return View(WikiMarkupFileProvider.Prefix + existingPage.Id, existingPage);
			}

			return new ContentViewComponentResult("");
		}
	}
}