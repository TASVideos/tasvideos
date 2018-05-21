using System.Threading.Tasks;

using Microsoft.AspNetCore.Mvc;
using TASVideos.Data.Entity;
using TASVideos.Tasks;

namespace TASVideos.ViewComponents
{
	public class GameSubPages : ViewComponent
	{
		private readonly WikiTasks _wikiTasks;

		public GameSubPages(WikiTasks wikiTasks)
		{
			_wikiTasks = wikiTasks;
		}

		public async Task<IViewComponentResult> InvokeAsync(WikiPage pageData, string pp)
		{
			var model = await _wikiTasks.GetGameResourcesSubPages();
			return View(model);
		}
	}
}
