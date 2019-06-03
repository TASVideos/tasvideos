using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

using TASVideos.Data.Entity;
using TASVideos.Services;

namespace TASVideos.ViewComponents
{
	public class WikiTextChangeLog : ViewComponent
	{
		private readonly IWikiPages _wikiPages;

		public WikiTextChangeLog(IWikiPages wikiPages)
		{
			_wikiPages = wikiPages;
		}

		public async Task<IViewComponentResult> InvokeAsync(WikiPage pageData, string pp)
		{
			int limit = 50;
			bool includeMinorEdits = true;

			bool? paramIncludeMinorEdits = ParamHelper.GetBool(pp, "includeminors");
			if (paramIncludeMinorEdits.HasValue)
			{
				includeMinorEdits = paramIncludeMinorEdits.Value;
			}

			int? paramLimit = ParamHelper.GetInt(pp, "limit");
			if (paramLimit.HasValue)
			{
				limit = paramLimit.Value;
			}

			var results = await GetWikiChangeLog(limit, includeMinorEdits);
			return View(results);
		}

		private async Task<IEnumerable<WikiTextChangelogModel>> GetWikiChangeLog(int limit, bool includeMinorEdits)
		{
			var query = _wikiPages.Query
				.ThatAreNotDeleted()
				.ByMostRecent()
				.Take(limit);

			if (!includeMinorEdits)
			{
				query = query.ExcludingMinorEdits();
			}

			return await query
				.Select(wp => new WikiTextChangelogModel
				{
					PageName = wp.PageName,
					Revision = wp.Revision,
					Author = wp.CreateUserName,
					CreateTimestamp = wp.CreateTimeStamp,
					MinorEdit = wp.MinorEdit,
					RevisionMessage = wp.RevisionMessage
				})
				.ToListAsync();
		}
	}
}
