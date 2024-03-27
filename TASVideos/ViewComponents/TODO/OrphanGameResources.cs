using TASVideos.WikiEngine;

namespace TASVideos.ViewComponents.TODO;

[WikiModule(WikiModules.OrphanGameResources)]
public class OrphanGameResources(ApplicationDbContext db) : ViewComponent
{
	public async Task<IViewComponentResult> InvokeAsync()
	{
		var pages = await db.WikiPages
			.ThatAreNotDeleted()
			.ThatAreCurrent()
			.ForPageLevel(3)
			.ThatAreSubpagesOf("GameResources")
			.Select(wp => wp.PageName)
			.ToListAsync();

		// TODO: a join would be more efficient as the list grows
		var gamePages = await db.Games
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
