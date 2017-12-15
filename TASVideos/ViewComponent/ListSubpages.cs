using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using TASVideos.Tasks;
using TASVideos.Data.Entity;

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