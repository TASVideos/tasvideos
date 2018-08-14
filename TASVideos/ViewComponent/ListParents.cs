using System.Linq;
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
			var subpages = pageData.PageName.Contains('/')
				? await _wikiTasks.GetParents(pageData.PageName)
				: Enumerable.Empty<string>();

			return View(subpages);
		}
	}
}
