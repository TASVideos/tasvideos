using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using TASVideos.Data.Entity;
using TASVideos.Tasks;

namespace TASVideos.ViewComponents
{
	public class ListSubpages : ViewComponent
	{
		private readonly WikiTasks _wikiTasks;

		public ListSubpages(WikiTasks wikiTasks)
		{
			_wikiTasks = wikiTasks;
		}

		public async Task<IViewComponentResult> InvokeAsync(WikiPage pageData, string pp)
		{
			var subpages = await _wikiTasks.GetSubPages(pageData.PageName);
			return View(subpages);
		}
	}
}