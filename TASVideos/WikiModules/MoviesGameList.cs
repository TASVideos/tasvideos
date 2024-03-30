using TASVideos.Data.Entity.Game;
using TASVideos.WikiEngine;

namespace TASVideos.WikiModules;

[WikiModule(ModuleNames.MoviesGameList)]
public class MoviesGameList(ApplicationDbContext db) : ViewComponent
{
	public async Task<IViewComponentResult> InvokeAsync(int? system)
	{
		var systemObj = await db.GameSystems.SingleOrDefaultAsync(s => s.Id == system);
		if (system is null || systemObj is null)
		{
			return View(new MoviesGameListModel
			{
				SystemId = system,
			});
		}

		var model = new MoviesGameListModel
		{
			SystemId = system,
			SystemCode = systemObj.Code,
			Games = await db.Games
				.ForSystem(system.Value)
				.Select(g => new MoviesGameListModel.GameEntry
				{
					Id = g.Id,
					Name = g.DisplayName,
					PublicationIds = g.Publications
						.Where(p => p.ObsoletedById == null)
						.Select(p => p.Id)
						.ToList()
				})
				.ToListAsync()
		};

		return View(model);
	}

	public class MoviesGameListModel
	{
		public int? SystemId { get; init; }
		public string? SystemCode { get; init; }

		public IReadOnlyCollection<GameEntry> Games { get; init; } = [];
		public class GameEntry
		{
			public int Id { get; init; }
			public string Name { get; init; } = "";
			public IReadOnlyCollection<int> PublicationIds { get; init; } = [];
		}
	}
}
