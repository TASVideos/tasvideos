using TASVideos.Data.Entity.Game;
using TASVideos.WikiEngine;

namespace TASVideos.ViewComponents;

[WikiModule(WikiModules.MoviesList)]
public class MoviesList(ApplicationDbContext db) : ViewComponent
{
	public async Task<IViewComponentResult> InvokeAsync(string? platform)
	{
		var systemCode = platform;
		bool isAll = string.IsNullOrWhiteSpace(systemCode);
		GameSystem? system = null;

		if (!isAll)
		{
			system = await db.GameSystems.SingleOrDefaultAsync(s => s.Code == systemCode);
			if (system is null)
			{
				return View(new MoviesListModel { SystemCode = systemCode });
			}
		}

		var model = new MoviesListModel
		{
			SystemCode = systemCode,
			SystemName = system?.DisplayName ?? "ALL",
			Movies = await db.Publications
				.Where(p => isAll || p.System!.Code == systemCode)
				.Select(p => new MoviesListModel.MovieEntry
				{
					Id = p.Id,
					IsObsolete = p.ObsoletedById.HasValue,
					GameName = p.Game!.DisplayName
				})
				.ToListAsync()
		};

		return View(model);
	}
}
