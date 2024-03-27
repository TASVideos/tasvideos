using TASVideos.WikiEngine;

namespace TASVideos.ViewComponents;

[WikiModule(WikiModules.PlayerPointsTable)]
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
}
