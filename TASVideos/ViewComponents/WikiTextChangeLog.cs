using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TASVideos.Core.Services;
using TASVideos.Data.Entity;
using TASVideos.WikiEngine;

namespace TASVideos.ViewComponents;

[WikiModule(WikiModules.WikiTextChangeLog)]
public class WikiTextChangeLog : ViewComponent
{
	private readonly IWikiPages _wikiPages;

	public WikiTextChangeLog(IWikiPages wikiPages)
	{
		_wikiPages = wikiPages;
	}

	public async Task<IViewComponentResult> InvokeAsync(bool includeMinors, int? limit)
	{
		var results = await GetWikiChangeLog(limit ?? 50, includeMinors);
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
				CreateTimestamp = wp.CreateTimestamp,
				MinorEdit = wp.MinorEdit,
				RevisionMessage = wp.RevisionMessage
			})
			.ToListAsync();
	}
}
