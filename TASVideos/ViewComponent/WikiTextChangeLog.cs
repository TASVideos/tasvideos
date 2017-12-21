using System;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using TASVideos.Tasks;
using TASVideos.Data.Entity;

namespace TASVideos.ViewComponents
{
	public class WikiTextChangeLog : ViewComponent
	{
		private readonly WikiTasks _wikiTasks;

		public WikiTextChangeLog(WikiTasks wikiTasks)
		{
			_wikiTasks = wikiTasks;
		}

		public async Task<IViewComponentResult> InvokeAsync(WikiPage pageData, string pp)
		{
			// TODO: parse shenanigans
			var args = pp?.Split('|');

			int limit = 50;
			bool includeMinorEdits = true;

			var results = await _wikiTasks.GetWikiChangeLog(limit, includeMinorEdits);
			return View(results);

		}
	}
}
