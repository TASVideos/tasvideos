using TASVideos.WikiEngine;

namespace TASVideos.WikiModules;

[WikiModule(ModuleNames.GameName)]
public class GameName(ApplicationDbContext db) : WikiViewComponent
{
	public List<GameEntry> Games { get; set; } = [];

	public async Task<IViewComponentResult> InvokeAsync()
	{
		if (CurrentPage.IsSystemGameResourcePath())
		{
			var system = await db.GameSystems
				.SingleOrDefaultAsync(s => s.Code == CurrentPage.SystemGameResourcePath());
			Games.Add(new GameEntry
			{
				System = system is not null
					? system.DisplayName
					: "various"
			});
		}
		else
		{
			var baseGame = string.Join("/", CurrentPage.Split('/').Take(3));
			Games = await db.Games
				.Where(g => g.GameResourcesPage == baseGame)
				.Select(g => new GameEntry
				{
					GameId = g.Id,
					DisplayName = g.DisplayName
				})
				.ToListAsync();
		}

		return View();
	}

	public class GameEntry
	{
		public int GameId { get; init; }
		public string DisplayName { get; init; } = "";
		public string? System { get; init; }
		public bool IsSystem => !string.IsNullOrWhiteSpace(System);
	}
}
