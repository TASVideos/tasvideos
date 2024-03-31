using TASVideos.WikiEngine;

namespace TASVideos.WikiModules;

[WikiModule(ModuleNames.OrphanGameResources)]
public class OrphanGameResources(ApplicationDbContext db) : WikiViewComponent
{
	public List<string> Pages { get; set; } = [];

	public async Task<IViewComponentResult> InvokeAsync()
	{
		Pages = await db.WikiPages
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

		Pages = Pages
			.Where(p => !gamePages.Contains(p))
			.ToList();

		return View();
	}
}
