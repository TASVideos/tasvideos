using TASVideos.Data.Entity.Game;
using TASVideos.WikiEngine;

namespace TASVideos.WikiModules;

[WikiModule(ModuleNames.MoviesList)]
public class MoviesList(ApplicationDbContext db) : WikiViewComponent
{
	public string? SystemCode { get; set; }
	public string? SystemName { get; set; }
	public List<MovieEntry> Movies { get; set; } = [];

	public async Task<IViewComponentResult> InvokeAsync(string? platform)
	{
		SystemCode = platform;
		bool isAll = string.IsNullOrWhiteSpace(SystemCode);
		GameSystem? system = null;

		if (!isAll)
		{
			system = await db.GameSystems.SingleOrDefaultAsync(s => s.Code == SystemCode);
			if (system is null)
			{
				return View();
			}
		}

		SystemName = system?.DisplayName ?? "ALL";
		Movies = await db.Publications
			.Where(p => isAll || p.System!.Code == SystemCode)
			.Select(p => new MovieEntry(
				p.Id,
				p.ObsoletedById.HasValue,
				p.Game!.DisplayName))
			.ToListAsync();

		return View();
	}

	public record MovieEntry(int Id, bool IsObsolete, string GameName);
}
