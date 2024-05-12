using TASVideos.WikiEngine;

namespace TASVideos.WikiModules;

[WikiModule(ModuleNames.DisplayGameName)]
public class DisplayGameName(ApplicationDbContext db) : WikiViewComponent
{
	public async Task<IViewComponentResult> InvokeAsync(IList<int> gid)
	{
		if (!gid.Any())
		{
			return Error("No game ID specified");
		}

		var games = await db.Games
			.Where(g => gid.Contains(g.Id))
			.OrderBy(d => d)
			.ToListAsync();

		var displayNames = games
			.OrderBy(g => g.DisplayName)
			.Select(g => $"{g.DisplayName}");

		return String(string.Join(", ", displayNames));
	}
}
