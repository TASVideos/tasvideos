using Microsoft.AspNetCore.Mvc.ViewComponents;
using TASVideos.WikiEngine;

namespace TASVideos.WikiModules;

[WikiModule(ModuleNames.MovieMaintenanceLog)]
public class MovieMaintenanceLog(ApplicationDbContext db) : WikiViewComponent
{
	public IEnumerable<IGrouping<ParentPublicationMaintenanceEntry, PublicationMaintenanceLogEntry>> Global { get; set; } = [];
	public IEnumerable<PublicationMaintenanceLogEntry> Default { get; set; } = [];
	public bool DefaultMode { get; set; }
	public async Task<IViewComponentResult> InvokeAsync()
	{
		if (int.TryParse(Request.Query["id"], out int publicationId))
		{
			var publicationTitle = await db.Publications
				.Where(p => p.Id == publicationId)
				.Select(p => p.Title)
				.SingleOrDefaultAsync();

			if (publicationTitle is null)
			{
				return new ContentViewComponentResult($"Publication #{publicationId} not found.");
			}

			ViewData["pubTitle"] = publicationTitle;
			ViewData["pubId"] = publicationId;

			Default = await db.PublicationMaintenanceLogs
				.Where(l => l.PublicationId == publicationId)
				.Select(l => new PublicationMaintenanceLogEntry(l.Log, l.User!.UserName, l.TimeStamp))
				.ToListAsync();
			DefaultMode = true;
			return View();
		}

		const int pageSize = 50;
		int.TryParse(Request.Query["begin"], out int begin);
		ViewData["next"] = begin + pageSize;

		IEnumerable<IGrouping<ParentPublicationMaintenanceEntry, PublicationMaintenanceLogEntry>> globalEntries = (await db.PublicationMaintenanceLogs
			.OrderByDescending(l => l.TimeStamp)
			.Skip(begin)
			.Take(pageSize)
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
				gkey => new ParentPublicationMaintenanceEntry(gkey.PublicationId, gkey.PublicationTitle),
				gvalue => new PublicationMaintenanceLogEntry(gvalue.Log, gvalue.UserName, gvalue.TimeStamp));
		Global = globalEntries;
		return View();
	}

	public record PublicationMaintenanceLogEntry(string Log, string UserName, DateTime Timestamp);
	public record ParentPublicationMaintenanceEntry(int Id, string Title);
}
