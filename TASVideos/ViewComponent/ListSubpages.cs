using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using TASVideos.Data.Entity;
using TASVideos.Tasks;

namespace TASVideos.ViewComponents
{
	public class ListSubPages : ViewComponent
	{
		private readonly WikiTasks _wikiTasks;

		public ListSubPages(WikiTasks wikiTasks)
		{
			_wikiTasks = wikiTasks;
		}

		public async Task<IViewComponentResult> InvokeAsync(WikiPage pageData, string pp)
		{
			var subpages = await _wikiTasks.GetSubPages(pageData.PageName);
			ViewData["Parent"] = pageData.PageName;
			return View(subpages);
		}
	}
}