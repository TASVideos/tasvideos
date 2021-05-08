using System.Linq;
using Microsoft.AspNetCore.Mvc;
using TASVideos.Core.Services;
using TASVideos.Data.Entity;
using TASVideos.WikiEngine;

namespace TASVideos.ViewComponents
{
	// TODO: remove instances of this module and retire it as a module (still a view component but no need to be in the wiki markup since it is automatic)
	[WikiModule(WikiModules.ListSubPages)]
	public class ListSubPages : ViewComponent
	{
		private readonly IWikiPages _wikiPages;

		public ListSubPages(IWikiPages wikiPages)
		{
			_wikiPages = wikiPages;
		}

		public IViewComponentResult Invoke(WikiPage pageData, bool show)
		{
			if (string.IsNullOrWhiteSpace(pageData.PageName))
			{
				return Content("");
			}

			var subpages = _wikiPages.Query
				.ThatAreSubpagesOf(pageData.PageName)
				.Select(w => w.PageName)
				.ToList();

			ViewData["Parent"] = pageData.PageName;

			if (show)
			{
				ViewData["show"] = true;
			}

			return View(subpages);
		}
	}
}
