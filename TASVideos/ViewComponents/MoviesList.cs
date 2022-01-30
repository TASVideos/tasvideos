using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TASVideos.Data;
using TASVideos.Data.Entity.Game;
using TASVideos.WikiEngine;

namespace TASVideos.ViewComponents;

[WikiModule(WikiModules.MoviesList)]
public class MoviesList : ViewComponent
{
	private readonly ApplicationDbContext _db;

	public MoviesList(ApplicationDbContext db)
	{
		_db = db;
	}

	public async Task<IViewComponentResult> InvokeAsync(string? platform)
	{
		var systemCode = platform;
		bool isAll = string.IsNullOrWhiteSpace(systemCode);
		GameSystem? system = null;

		if (!isAll)
		{
			system = await _db.GameSystems.SingleOrDefaultAsync(s => s.Code == systemCode);
			if (system == null)
			{
				return View(new MoviesListModel { SystemCode = systemCode });
			}
		}

		var model = new MoviesListModel
		{
			SystemCode = systemCode,
			SystemName = system?.DisplayName ?? "ALL",
			Movies = await _db.Publications
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
