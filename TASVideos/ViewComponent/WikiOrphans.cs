using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using TASVideos.Data.Entity;
using TASVideos.Tasks;

namespace TASVideos.ViewComponents
{
	public class WikiOrphans : ViewComponent
	{
		private readonly WikiTasks _wikiTasks;

		public WikiOrphans(WikiTasks wikiTasks)
		{
			_wikiTasks = wikiTasks;
		}

		public async Task<IViewComponentResult> InvokeAsync(WikiPage pageData, string pp)
		{
			var orphans = await _wikiTasks.GetAllOrphans();
			return View(orphans);
		}
	}
}
