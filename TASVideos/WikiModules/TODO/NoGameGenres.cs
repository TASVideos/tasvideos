using TASVideos.WikiModules.Models;
using TASVideos.WikiEngine;

namespace TASVideos.WikiModules.TODO;

[WikiModule(ModuleNames.NoGameGenre)]
public class NoGameGenres(ApplicationDbContext db) : ViewComponent
{
	public async Task<IViewComponentResult> InvokeAsync()
	{
		var games = await db.Games
			.Where(g => g.GameGenres.Count == 0)
			.Select(g => new GameEntry(g.Id, g.DisplayName))
			.ToListAsync();

		return View(games);
	}
}
