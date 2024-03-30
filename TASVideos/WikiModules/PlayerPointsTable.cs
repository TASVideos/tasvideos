using TASVideos.WikiEngine;

namespace TASVideos.WikiModules;

[WikiModule(ModuleNames.PlayerPointsTable)]
public class PlayerPointsTable(ApplicationDbContext db, IPointsService pointsService) : ViewComponent
{
	public async Task<IViewComponentResult> InvokeAsync(int? count)
	{
		var showCount = count ?? 50;

		var players = await db.Users
			.ThatArePublishedAuthors()
			.Select(u => new PlayerPointsModel
			{
				Id = u.Id,
				Player = u.UserName
			})
			.ToListAsync();

		foreach (var user in players)
		{
			(user.Points, user.Rank) = await pointsService.PlayerPoints(user.Id);
		}

		var sortedPlayers = players
			.OrderByDescending(u => u.Points)
			.Take(showCount)
			.ToList();

		int counter = 0;
		foreach (var user in sortedPlayers)
		{
			user.Position = ++counter;
		}

		return View(sortedPlayers);
	}

	public class PlayerPointsModel
	{
		[Display(Name = "Pos")]
		public int Position { get; set; } = 0;

		[Display(Name = "PlayerID")]
		public int Id { get; init; } = 0;

		[Display(Name = "Player")]
		public string Player { get; init; } = "";

		[Display(Name = "Points")]
		public double Points { get; set; } = 0.0;

		[Display(Name = "Player Rank")]
		public string Rank { get; set; } = "";
	}
}
