using System.Linq;
using Microsoft.AspNetCore.Mvc;
using TASVideos.Data.Entity;
using TASVideos.Services;
using TASVideos.WikiEngine;

namespace TASVideos.ViewComponents
{
	// TODO: remove instances of this module and retire it as a module (still a view component but no need to be in the wiki markup since it is automatic)
	[WikiModule(WikiModules.ListParents)]
	public class ListParents : ViewComponent
	{
		private readonly IWikiPages _wikiPages;

		public ListParents(IWikiPages wikiPages)
		{
			_wikiPages = wikiPages;
		}

		public IViewComponentResult Invoke(WikiPage pageData)
		{
			var subpages = _wikiPages.Query
					.ThatAreParentsOf(pageData.PageName)
					.ToList();

			return View(subpages);
		}
	}
}
