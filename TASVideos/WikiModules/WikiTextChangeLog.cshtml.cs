using TASVideos.WikiEngine;

namespace TASVideos.WikiModules;

[WikiModule(ModuleNames.WikiTextChangeLog)]
public class WikiTextChangeLog(ApplicationDbContext db) : WikiViewComponent
{
	public PageOf<Log> Logs { get; set; } = new([], new());

	public async Task<IViewComponentResult> InvokeAsync(bool includeMinors)
	{
		DefaultPageSize = 100;

		var query = db.WikiPages
			.ThatAreNotDeleted()
			.ByMostRecent();

		if (!includeMinors)
		{
			query = query.ExcludingMinorEdits();
		}

		Logs = await query
			.Select(wp => new Log(
				wp.CreateTimestamp,
				wp.Author!.UserName,
				wp.PageName,
				wp.Revision,
				wp.MinorEdit,
				wp.RevisionMessage))
			.PageOf(GetPaging());

		return View();
	}

	public record Log(
		DateTime CreateTimestamp,
		string? Author,
		string PageName,
		int Revision,
		bool MinorEdit,
		string? RevisionMessage);
}
