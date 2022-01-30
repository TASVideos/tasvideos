using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TASVideos.Data;
using TASVideos.ViewComponents.Models;
using TASVideos.WikiEngine;

namespace TASVideos.ViewComponents;

[WikiModule(WikiModules.NoGameName)]
public class NoGame : ViewComponent
{
	private readonly ApplicationDbContext _db;

	public NoGame(ApplicationDbContext db)
	{
		_db = db;
	}

	public async Task<IViewComponentResult> InvokeAsync()
	{
		var model = new MissingRomModel
		{
			Publications = await _db.Publications
				.Where(p => p.GameId == -1)
				.OrderBy(p => p.Id)
				.Select(p => new MissingRomModel.Entry(p.Id, p.Title))
				.ToListAsync(),
			Submissions = await _db.Submissions
				.Where(s => s.GameId == null || s.GameId < 1)
				.OrderBy(p => p.Id)
				.Select(s => new MissingRomModel.Entry(s.Id, s.Title))
				.ToListAsync()
		};

		return View(model);
	}
}
