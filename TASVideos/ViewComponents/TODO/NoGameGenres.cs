using Microsoft.AspNetCore.Mvc;
using TASVideos.Data;
using TASVideos.ViewComponents.Models;
using TASVideos.WikiEngine;

namespace TASVideos.ViewComponents.TODO;

[WikiModule(WikiModules.NoGameGenre)]
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
