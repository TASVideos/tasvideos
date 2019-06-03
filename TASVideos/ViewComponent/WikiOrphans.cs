using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;

using TASVideos.Data.Entity;
using TASVideos.Services;

namespace TASVideos.ViewComponents
{
	public class WikiOrphans : ViewComponent
	{
		private readonly IWikiPages _wikiPages;

		public WikiOrphans(IWikiPages wikiPages)
		{
			_wikiPages = wikiPages;
		}

		public async Task<IViewComponentResult> InvokeAsync(WikiPage pageData, string pp)
		{
			var orphans = await _wikiPages.Orphans();
			return View(orphans);
		}
	}
}
