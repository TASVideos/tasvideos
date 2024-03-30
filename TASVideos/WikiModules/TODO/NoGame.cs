using TASVideos.WikiModules.Models;
using TASVideos.WikiEngine;

namespace TASVideos.WikiModules.TODO;

[WikiModule(ModuleNames.NoGameName)]
public class NoGame(ApplicationDbContext db) : ViewComponent
{
	public async Task<IViewComponentResult> InvokeAsync()
	{
		var model = new MissingModel
		{
			Publications = await db.Publications
				.Where(p => p.GameId == -1)
				.OrderBy(p => p.Id)
				.Select(p => new MissingModel.Entry(p.Id, p.Title))
				.ToListAsync(),
			Submissions = await db.Submissions
				.Where(s => s.GameId == null || s.GameId < 1)
				.ThatAreInActive()
				.OrderBy(p => p.Id)
				.Select(s => new MissingModel.Entry(s.Id, s.Title))
				.ToListAsync()
		};

		return View(model);
	}
}
