using TASVideos.Data.Entity.Game;
using TASVideos.WikiEngine;

namespace TASVideos.WikiModules;

[WikiModule(ModuleNames.MoviesGameList)]
public class MoviesGameList(ApplicationDbContext db) : WikiViewComponent
{
	public int? SystemId { get; set; }
	public string? SystemCode { get; set; }
	public List<GameEntry> Games { get; set; } = [];

	public async Task<IViewComponentResult> InvokeAsync(int? system)
	{
		SystemId = system;
		var systemObj = await db.GameSystems.SingleOrDefaultAsync(s => s.Id == system);
		if (SystemId is null || systemObj is null)
		{
			return View();
		}

		SystemCode = systemObj.Code;

		Games = await db.Games
			.ForSystem(SystemId.Value)
			.Select(g => new GameEntry(
				g.Id,
				g.DisplayName,
				g.Publications
					.Where(p => p.ObsoletedById == null)
					.Select(p => p.Id)
					.ToList()))
			.ToListAsync();

		return View();
	}

	public record GameEntry(int Id, string Name, List<int> PublicationIds);
}
