using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using TASVideos.Tasks;
using TASVideos.Data.Entity;


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
