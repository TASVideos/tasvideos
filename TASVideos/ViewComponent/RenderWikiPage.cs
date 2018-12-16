using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ViewComponents;

using TASVideos.Extensions;
using TASVideos.Razor;
using TASVideos.Services;

namespace TASVideos.ViewComponents
{
	public class RenderWikiPage : ViewComponent
	{
		private readonly IWikiService _wikiService;
		private readonly WikiMarkupFileProvider _wikiMarkupFileProvider;

		public RenderWikiPage(
			IWikiService wikiService,
			WikiMarkupFileProvider wikiMarkupFileProvider)
		{
			_wikiService = wikiService;
			_wikiMarkupFileProvider = wikiMarkupFileProvider;
		}

		public IViewComponentResult Invoke(string url, int? revision = null)
		{
			url = url.Trim('/');
			if (!WikiHelper.IsValidWikiPageName(url))
			{
				return new ContentViewComponentResult("");
			}

			var existingPage = _wikiService.Page(url, revision);

			if (existingPage != null)
			{
				ViewData["WikiPage"] = existingPage;
				ViewData["Title"] = existingPage.PageName;
				ViewData["Layout"] = null;
				_wikiMarkupFileProvider.WikiService = _wikiService;
				return View(WikiMarkupFileProvider.Prefix + existingPage.Id, existingPage);
			}

			return new ContentViewComponentResult("");
		}
	}
}