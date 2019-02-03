using System.Linq;
using Microsoft.AspNetCore.Mvc;
using TASVideos.Data.Entity;
using TASVideos.Services;

namespace TASVideos.ViewComponents
{
	public class ListSubPages : ViewComponent
	{
		private readonly IWikiPages _wikiPages;

		public ListSubPages(IWikiPages wikiPages)
		{
			_wikiPages = wikiPages;
		}

		public IViewComponentResult Invoke(WikiPage pageData, string pp = null)
		{
			if (pageData == null)
			{
				return Content("");
			}

			var subpages = _wikiPages
				.ThatAreSubpagesOf(pageData.PageName)
				.Select(w => w.PageName)
				.ToList();

			ViewData["Parent"] = pageData.PageName;
			
			if (pp?.Contains("show") ?? false)
			{
				ViewData["show"] = true;
			}

			return View(subpages);
		}
	}
}