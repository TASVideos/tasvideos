using TASVideos.WikiEngine;

namespace TASVideos.WikiModules;

[WikiModule(ModuleNames.NoGameName)]
public class NoGame(ApplicationDbContext db) : WikiViewComponent
{
	public List<Entry> Publications { get; set; } = [];
	public List<Entry> Submissions { get; set; } = [];

	public async Task<IViewComponentResult> InvokeAsync()
	{
		Publications = await db.Publications
			.Where(p => p.GameId == -1)
			.OrderBy(p => p.Id)
			.Select(p => new Entry(p.Id, p.Title))
			.ToListAsync();
		Submissions = await db.Submissions
			.Where(s => s.GameId == null)
			.ThatAreInActive()
			.OrderBy(p => p.Id)
			.Select(s => new Entry(s.Id, s.Title))
			.ToListAsync();

		return View();
	}

	public record Entry(int Id, string Title);
}
