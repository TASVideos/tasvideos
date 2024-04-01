using TASVideos.WikiEngine;

namespace TASVideos.WikiModules;

[WikiModule(ModuleNames.NoGameVersion)]
public class NoGameVersion(ApplicationDbContext db) : WikiViewComponent
{
	public List<NoGame.Entry> Publications { get; set; } = [];
	public List<NoGame.Entry> Submissions { get; set; } = [];

	public async Task<IViewComponentResult> InvokeAsync()
	{
		Publications = await db.Publications
			.Where(p => p.GameVersionId == -1)
			.OrderBy(p => p.Id)
			.Select(p => new NoGame.Entry(p.Id, p.Title))
			.ToListAsync();
		Submissions = await db.Submissions
			.Where(s => s.GameVersionId == null || s.GameVersionId < 1)
			.ThatAreInActive()
			.OrderBy(p => p.Id)
			.Select(s => new NoGame.Entry(s.Id, s.Title))
			.ToListAsync();

		return View();
	}
}
