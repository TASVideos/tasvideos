using TASVideos.WikiEngine;

namespace TASVideos.WikiModules;

[WikiModule(ModuleNames.GameSubPages)]
public class GameSubPages(ApplicationDbContext db) : WikiViewComponent
{
	public List<Entry> Pages { get; set; } = [];

	public async Task<IViewComponentResult> InvokeAsync()
	{
		Pages = await GetGameResourcesSubPages();
		return View();
	}

	private async Task<List<Entry>> GetGameResourcesSubPages()
	{
		// TODO: cache this
		var systems = await db.GameSystems.ToListAsync();
		var gameResourceSystems = systems.Select(s => "GameResources/" + s.Code);

		var pages = db.WikiPages
			.ThatAreNotDeleted()
			.ThatAreCurrent()
			.Where(wp => gameResourceSystems.Contains(wp.PageName))
			.Select(wp => wp.PageName)
			.ToList();

		return
			(from s in systems
			 join wp in pages on s.Code equals wp.Split('/').Last()
			 select new Entry
			 {
				 SystemCode = s.Code,
				 SystemDescription = s.DisplayName,
				 PageLink = "GameResources/" + s.Code
			 })
			.ToList();
	}

	public class Entry
	{
		public string SystemCode { get; init; } = "";
		public string SystemDescription { get; init; } = "";
		public string PageLink { get; init; } = "";
	}
}
