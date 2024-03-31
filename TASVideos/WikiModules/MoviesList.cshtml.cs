using TASVideos.Data.Entity.Game;
using TASVideos.WikiEngine;

namespace TASVideos.WikiModules;

[WikiModule(ModuleNames.MoviesList)]
public class MoviesList(ApplicationDbContext db) : WikiViewComponent
{
	public MoviesListModel List { get; set; } = new();

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
				List = new MoviesListModel { SystemCode = systemCode };
				return View();
			}
		}

		List = new MoviesListModel
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

		return View();
	}

	public class MoviesListModel
	{
		public string? SystemCode { get; init; }
		public string? SystemName { get; init; }

		public IReadOnlyCollection<MovieEntry> Movies { get; init; } = [];

		public class MovieEntry
		{
			public int Id { get; init; }
			public bool IsObsolete { get; init; }
			public string GameName { get; init; } = "";
		}
	}
}
