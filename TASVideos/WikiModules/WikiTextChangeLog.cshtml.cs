using TASVideos.Core;
using TASVideos.WikiEngine;

namespace TASVideos.WikiModules;

[WikiModule(ModuleNames.WikiTextChangeLog)]
public class WikiTextChangeLog(ApplicationDbContext db) : WikiViewComponent
{
	public PageOf<Entry> Log { get; set; } = PageOf<Entry>.Empty();

	public async Task<IViewComponentResult> InvokeAsync(bool includeMinors)
	{
		var paging = this.GetPagingModel(100);
		Log = await GetWikiChangeLog(paging, includeMinors);
		this.SetPagingToViewData(paging);
		return View();
	}

	private async Task<PageOf<Entry>> GetWikiChangeLog(PagingModel paging, bool includeMinorEdits)
	{
		var query = db.WikiPages
			.ThatAreNotDeleted()
			.ByMostRecent();

		if (!includeMinorEdits)
		{
			query = query.ExcludingMinorEdits();
		}

		return await query
			.Select(wp => new Entry
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

	public class Entry
	{
		public DateTime CreateTimestamp { get; init; }
		public string? Author { get; init; }
		public string PageName { get; init; } = "";
		public int Revision { get; init; }
		public bool MinorEdit { get; init; }
		public string? RevisionMessage { get; init; }
	}
}
