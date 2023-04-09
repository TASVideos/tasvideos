using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TASVideos.Data;
using TASVideos.ViewComponents.Models;
using TASVideos.WikiEngine;

namespace TASVideos.ViewComponents.TODO;

[WikiModule(WikiModules.NoGameGenre)]
public class NoGameGenres : ViewComponent
{
	private readonly ApplicationDbContext _db;

	public NoGameGenres(ApplicationDbContext db)
	{
		_db = db;
	}

	public async Task<IViewComponentResult> InvokeAsync()
	{
		var games = await _db.Games
			.Where(g => g.GameGenres.Count == 0)
			.Select(g => new GameEntry(g.Id, g.DisplayName))
			.ToListAsync();

		return View(games);
	}
}
