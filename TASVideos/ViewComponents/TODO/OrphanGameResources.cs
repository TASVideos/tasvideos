using Microsoft.AspNetCore.Mvc;
using TASVideos.Data;
using TASVideos.Data.Entity;
using TASVideos.WikiEngine;

namespace TASVideos.ViewComponents.TODO;

[WikiModule(WikiModules.OrphanGameResources)]
public class OrphanGameResources : ViewComponent
{
	private readonly ApplicationDbContext _db;

	public OrphanGameResources(ApplicationDbContext db)
	{
		_db = db;
	}

	public async Task<IViewComponentResult> InvokeAsync()
	{
		var pages = await _db.WikiPages
			.ThatAreNotDeleted()
			.ThatAreCurrent()
			.ForPageLevel(3)
			.ThatAreSubpagesOf("GameResources")
			.Select(wp => wp.PageName)
			.ToListAsync();

		// TODO: a join would be more efficient as the list grows
		var gamePages = await _db.Games
			.Where(g => g.GameResourcesPage != null)
			.Select(g => g.GameResourcesPage)
			.Distinct()
			.ToListAsync();

		pages = pages
			.Where(p => !gamePages.Contains(p))
			.ToList();

		return View(pages);
	}
}
