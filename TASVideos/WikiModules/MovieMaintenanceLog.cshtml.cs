using TASVideos.WikiEngine;

namespace TASVideos.WikiModules;

[WikiModule(ModuleNames.MovieMaintenanceLog)]
public class MovieMaintenanceLog(ApplicationDbContext db) : WikiViewComponent
{
	public IEnumerable<IGrouping<ParentLogEntry, LogEntry>> Logs { get; set; } = [];
	public bool IsSinglePub { get; set; }
	public int Next { get; set; }
	public async Task<IViewComponentResult> InvokeAsync()
	{
		IsSinglePub = int.TryParse(Request.Query["id"], out int publicationId);

		const int pageSize = 50;
		int.TryParse(Request.Query["begin"], out int begin);
		Next = begin + pageSize;

		var query = db.PublicationMaintenanceLogs.OrderByDescending(l => l.TimeStamp).AsQueryable();
		query = IsSinglePub
			? query.Where(p => p.PublicationId == publicationId)
			: query.Skip(begin).Take(pageSize);

		Logs = (await query
			.Select(l => new
			{
				l.PublicationId,
				PublicationTitle = l.Publication!.Title,
				l.Log,
				l.TimeStamp,
				l.User!.UserName
			})
			.ToListAsync())
			.GroupBy(
				gkey => new ParentLogEntry(gkey.PublicationId, gkey.PublicationTitle),
				gvalue => new LogEntry(gvalue.Log, gvalue.UserName, gvalue.TimeStamp));
		return View();
	}

	public record LogEntry(string Log, string UserName, DateTime Timestamp);
	public record ParentLogEntry(int Id, string Title);
}
