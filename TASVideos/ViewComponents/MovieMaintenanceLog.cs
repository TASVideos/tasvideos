using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ViewComponents;
using Microsoft.EntityFrameworkCore;
using TASVideos.Data;
using TASVideos.WikiEngine;

namespace TASVideos.ViewComponents;

[WikiModule(WikiModules.MovieMaintenanceLog)]
public class MovieMaintenanceLog : ViewComponent
{
	private readonly ApplicationDbContext _db;

	public MovieMaintenanceLog(ApplicationDbContext db)
	{
		_db = db;
	}

	public async Task<IViewComponentResult> InvokeAsync()
	{
		if (int.TryParse(Request.Query["id"], out int publicationId))
		{
			var publicationTitle = await _db.Publications
				.Where(p => p.Id == publicationId)
				.Select(p => p.Title)
				.SingleOrDefaultAsync();

			if (publicationTitle == null)
			{
				return new ContentViewComponentResult($"Publication #{publicationId} not found.");
			}

			ViewData["title"] = publicationTitle;
			ViewData["id"] = publicationId;

			var entries = await _db.PublicationMaintenanceLogs
				.Where(l => l.PublicationId == publicationId)
				.Select(l => new PublicationMaintenanceLogEntry(l.Log, l.User!.UserName, l.TimeStamp))
				.ToListAsync();

			return View("Default", entries);
		}

		const int pageSize = 50;
		int.TryParse(Request.Query["begin"], out int begin);
		ViewData["next"] = begin + pageSize;

		IEnumerable<IGrouping<ParentPublicationMaintenanceEntry, PublicationMaintenanceLogEntry>> globalEntries = (await _db.PublicationMaintenanceLogs
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

		return View("Global", globalEntries);
	}
}
