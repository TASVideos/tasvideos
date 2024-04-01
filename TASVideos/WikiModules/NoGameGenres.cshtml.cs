using TASVideos.WikiEngine;

namespace TASVideos.WikiModules;

[WikiModule(ModuleNames.NoGameGenre)]
public class NoGameGenres(ApplicationDbContext db) : WikiViewComponent
{
	public List<Entry> Games { get; set; } = [];

	public async Task<IViewComponentResult> InvokeAsync()
	{
		Games = await db.Games
			.Where(g => g.GameGenres.Count == 0)
			.Select(g => new Entry(g.Id, g.DisplayName))
			.ToListAsync();

		return View();
	}

	public record Entry(int Id, string DisplayName);
}
