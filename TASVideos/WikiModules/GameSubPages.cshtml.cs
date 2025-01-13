using TASVideos.WikiEngine;

namespace TASVideos.WikiModules;

[WikiModule(ModuleNames.GameSubPages)]
public class GameSubPages(ApplicationDbContext db) : WikiViewComponent
{
	public List<Entry> Pages { get; set; } = [];

	public async Task<IViewComponentResult> InvokeAsync()
	{
		// TODO: cache this
		var systems = await db.GameSystems.ToListAsync();
		var gameResourceSystems = systems.Select(s => "GameResources/" + s.Code);

		var pages = await db.WikiPages
			.ThatAreNotDeleted()
			.ThatAreCurrent()
			.Where(wp => gameResourceSystems.Contains(wp.PageName))
			.Select(wp => wp.PageName)
			.ToListAsync();

		Pages = systems
			.Join(pages, s => s.Code, wp => wp.Split('/').Last(), (s, _) => s)
			.Select(s => new Entry(s.Code, s.DisplayName, "GameResources/" + s.Code))
			.ToList();

		return View();
	}

	public record Entry(string SystemCode, string SystemDescription, string PageLink);
}
