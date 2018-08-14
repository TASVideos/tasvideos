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

		public IViewComponentResult Invoke(WikiPage pageData, string pp = null)
		{
			var subpages = _wikiTasks.GetSubPages(pageData.PageName);
			ViewData["Parent"] = pageData.PageName;
			
			if (pp?.Contains("show") ?? false)
			{
				ViewData["show"] = true;
			}

			return View(subpages);
		}
	}
}