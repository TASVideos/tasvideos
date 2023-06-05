using Microsoft.AspNetCore.Mvc;
using TASVideos.Data;
using TASVideos.Data.Entity;
using TASVideos.ViewComponents.Models;
using TASVideos.WikiEngine;

namespace TASVideos.ViewComponents.TODO;

[WikiModule(WikiModules.NoGameVersion)]
public class NoGameVersion : ViewComponent
{
	private readonly ApplicationDbContext _db;

	public NoGameVersion(ApplicationDbContext db)
	{
		_db = db;
	}

	public async Task<IViewComponentResult> InvokeAsync()
	{
		var model = new MissingModel
		{
			Publications = await _db.Publications
				.Where(p => p.GameVersionId == -1)
				.OrderBy(p => p.Id)
				.Select(p => new MissingModel.Entry(p.Id, p.Title))
				.ToListAsync(),
			Submissions = await _db.Submissions
				.Where(s => s.GameVersionId == null || s.GameVersionId < 1)
				.ThatAreInActive()
				.OrderBy(p => p.Id)
				.Select(s => new MissingModel.Entry(s.Id, s.Title))
				.ToListAsync()
		};
		return View(model);
	}
}
