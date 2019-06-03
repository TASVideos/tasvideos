using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;

using TASVideos.Data.Entity;
using TASVideos.Services;

namespace TASVideos.ViewComponents
{
	public class BrokenLinks : ViewComponent
	{
		private readonly IWikiPages _wikiPages;

		public BrokenLinks(IWikiPages wikiPages)
		{
			_wikiPages = wikiPages;
		}

		public async Task<IViewComponentResult> InvokeAsync(WikiPage pageData, string pp)
		{
			var brokenLinks = await _wikiPages.BrokenLinks();
			return View(brokenLinks);
		}
	}
}