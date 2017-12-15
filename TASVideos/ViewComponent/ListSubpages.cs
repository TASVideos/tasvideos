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

		public IViewComponentResult Invoke(WikiPage pageData, string pp)
		{
			var subpages = new[] { "heh", "heh", "heh" };
			return View(subpages);
		}
	}
}