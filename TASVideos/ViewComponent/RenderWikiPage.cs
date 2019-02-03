using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ViewComponents;

using TASVideos.Extensions;
using TASVideos.Services;

namespace TASVideos.ViewComponents
{
	public class RenderWikiPage : ViewComponent
	{
		private readonly IWikiPages _wikiPages;

		public RenderWikiPage(
			IWikiPages wikiPages)
		{
			_wikiPages = wikiPages;
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
				var model = new RenderWikiPageModel
				{
					Markup = existingPage.Markup,
					PageData = existingPage
				};
				ViewData["WikiPage"] = existingPage;
				ViewData["Title"] = existingPage.PageName;
				ViewData["Layout"] = null;
				return View(model);
			}

			return new ContentViewComponentResult("");
		}
	}
}