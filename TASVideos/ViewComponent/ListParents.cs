using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using TASVideos.Data.Entity;
using TASVideos.Tasks;

namespace TASVideos.ViewComponents
{
	public class ListParents : ViewComponent
	{
		private readonly WikiTasks _wikiTasks;

		public ListParents(WikiTasks wikiTasks)
		{
			_wikiTasks = wikiTasks;
		}

		public async Task<IViewComponentResult> InvokeAsync(WikiPage pageData, string pp)
		{
			var subpages = await _wikiTasks.GetParents(pageData.PageName);
			return View(subpages);
		}
	}
}
