using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
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

		public async Task<IViewComponentResult> InvokeAsync(string pageName)
		{
			var subpages = await _wikiTasks.GetSubPages(pageName);
			return View(subpages);
		}
	}
}