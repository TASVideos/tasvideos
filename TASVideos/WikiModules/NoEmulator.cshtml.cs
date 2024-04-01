using TASVideos.WikiEngine;

namespace TASVideos.WikiModules;

[WikiModule(ModuleNames.NoEmulator)]
public class NoEmulator(ApplicationDbContext db) : WikiViewComponent
{
	public List<NoGame.Entry> Publications { get; set; } = [];
	public List<NoGame.Entry> Submissions { get; set; } = [];

	public async Task<IViewComponentResult> InvokeAsync()
	{
		Publications = await db.Publications
			.Where(p => string.IsNullOrEmpty(p.EmulatorVersion))
			.OrderBy(p => p.Id)
			.Select(p => new NoGame.Entry(p.Id, p.Title))
			.ToListAsync();
		Submissions = await db.Submissions
			.Where(s => string.IsNullOrEmpty(s.EmulatorVersion))
			.OrderBy(s => s.Id)
			.Select(s => new NoGame.Entry(s.Id, s.Title))
			.ToListAsync();

		return View();
	}
}
