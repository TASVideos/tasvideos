using TASVideos.Core;
using TASVideos.WikiEngine;

namespace TASVideos.WikiModules;

[WikiModule(ModuleNames.WikiTextChangeLog)]
public class WikiTextChangeLog(ApplicationDbContext db) : ViewComponent
{
	public async Task<IViewComponentResult> InvokeAsync(bool includeMinors)
	{
		var paging = this.GetPagingModel(100);
		var results = await GetWikiChangeLog(paging, includeMinors);
		this.SetPagingToViewData(paging);
		return View(results);
	}

	private async Task<PageOf<WikiTextChangelogModel>> GetWikiChangeLog(PagingModel paging, bool includeMinorEdits)
	{
		var query = db.WikiPages
			.ThatAreNotDeleted()
			.ByMostRecent();

		if (!includeMinorEdits)
		{
			query = query.ExcludingMinorEdits();
		}

		return await query
			.Select(wp => new WikiTextChangelogModel
			{
				PageName = wp.PageName,
				Revision = wp.Revision,
				Author = wp.Author!.UserName,
				CreateTimestamp = wp.CreateTimestamp,
				MinorEdit = wp.MinorEdit,
				RevisionMessage = wp.RevisionMessage
			})
			.PageOf(paging);
	}
}
