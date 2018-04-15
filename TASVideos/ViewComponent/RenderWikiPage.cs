using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ViewComponents;

using TASVideos.Extensions;
using TASVideos.Razor;
using TASVideos.Tasks;

namespace TASVideos.ViewComponents
{
	public class RenderWikiPage : ViewComponent
	{
		private readonly WikiTasks _wikiTasks;
		private readonly WikiMarkupFileProvider _wikiMarkupFileProvider;

		public RenderWikiPage(
			WikiTasks wikiTasks,
			WikiMarkupFileProvider wikiMarkupFileProvider)
		{
			_wikiTasks = wikiTasks;
			_wikiMarkupFileProvider = wikiMarkupFileProvider;
		}

		public async Task<IViewComponentResult> InvokeAsync(string url, int? revision = null)
		{
			url = url.Trim('/');
			if (!WikiHelper.IsValidWikiPageName(url))
			{
				return new ContentViewComponentResult("");
			}

			var existingPage = await _wikiTasks.GetPage(url, revision);

			if (existingPage != null)
			{
				ViewData["WikiPage"] = existingPage;
				ViewData["Title"] = existingPage.PageName;
				ViewData["Layout"] = null;
				_wikiMarkupFileProvider.WikiTasks = _wikiTasks;
				return View(Razor.WikiMarkupFileProvider.Prefix + existingPage.Id, existingPage);
			}

			return new ContentViewComponentResult("");
		}
	}
}