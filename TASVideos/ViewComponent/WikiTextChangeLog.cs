using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;

using TASVideos.Data.Entity;
using TASVideos.Extensions;
using TASVideos.Tasks;

namespace TASVideos.ViewComponents
{
	public class WikiTextChangeLog : ModuleComponentBase
	{
		private readonly WikiTasks _wikiTasks;

		public WikiTextChangeLog(WikiTasks wikiTasks)
		{
			_wikiTasks = wikiTasks;
		}

		public async Task<IViewComponentResult> InvokeAsync(WikiPage pageData, string pp)
		{
			int limit = 50;
			bool includeMinorEdits = true;

			bool? paramIncludeMinorEdits = GetBool(pp, "includeminors");
			if (paramIncludeMinorEdits.HasValue)
			{
				includeMinorEdits = paramIncludeMinorEdits.Value;
			}

			int? paramLimit = GetInt(pp, "limit");
			if (paramLimit.HasValue)
			{
				limit = paramLimit.Value;
			}

			var results = await _wikiTasks.GetWikiChangeLog(limit, includeMinorEdits);
			return View(results);
		}
	}
}
