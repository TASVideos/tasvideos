using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ViewComponents;
using Microsoft.EntityFrameworkCore;
using TASVideos.Data;
using TASVideos.Data.Entity;
using TASVideos.WikiEngine;

namespace TASVideos.ViewComponents;

[WikiModule(WikiModules.PlatformAuthorList)]
public class PlatformAuthorLists : ViewComponent
{
	private readonly ApplicationDbContext _db;

	public PlatformAuthorLists(ApplicationDbContext db)
	{
		_db = db;
	}

	public async Task<IViewComponentResult> InvokeAsync(bool showClassIcons, DateTime? before, DateTime? after, IList<int> platforms)
	{
		if (!before.HasValue || !after.HasValue)
		{
			return new ContentViewComponentResult("Invalid parameters.");
		}

		var model = new PlatformAuthorListModel
		{
			ShowClasses = showClassIcons,
			Publications = await _db.Publications
				.ForDateRange(before.Value, after.Value)
				.Where(p => !platforms.Any() || platforms.Contains(p.SystemId))
				.Select(p => new PlatformAuthorListModel.PublicationEntry
				{
					Id = p.Id,
					Title = p.Title,
					Authors = p.Authors.OrderBy(pa => pa.Ordinal).Select(pa => pa.Author!.UserName),
					ClassIconPath = p.PublicationClass!.IconPath
				})
				.ToListAsync()
		};

		return View(model);
	}
}
