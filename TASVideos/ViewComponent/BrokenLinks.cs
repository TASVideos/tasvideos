using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using TASVideos.Tasks;
using TASVideos.Data.Entity;

namespace TASVideos.ViewComponents
{
	public class BrokenLinks : ViewComponent
	{
		private readonly WikiTasks _wikiTasks;

		public BrokenLinks(WikiTasks wikiTasks)
		{
			_wikiTasks = wikiTasks;
		}

		public async Task<IViewComponentResult> InvokeAsync(WikiPage pageData, string pp)
		{
			var orphans = await _wikiTasks.GetAllBrokenLinks();
			return View(orphans);
		}
	}
}