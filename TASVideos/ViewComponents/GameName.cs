using Microsoft.AspNetCore.Mvc;
using TASVideos.Data;
using TASVideos.WikiEngine;

namespace TASVideos.ViewComponents;

[WikiModule(WikiModules.GameName)]
public class GameName(ApplicationDbContext db) : ViewComponent
{
	public async Task<IViewComponentResult> InvokeAsync()
	{
		var path = HttpContext.Request.Path.ToString().Trim('/');

		var gameList = new List<GameNameModel>();
		if (path.IsSystemGameResourcePath())
		{
			var system = await db.GameSystems
				.SingleOrDefaultAsync(s => s.Code == path.SystemGameResourcePath());
			gameList.Add(new GameNameModel
			{
				System = system is not null
					? system.DisplayName
					: "various"
			});
		}
		else
		{
			var baseGame = string.Join("/", path.Split('/').Take(3));
			gameList = await db.Games
				.Where(g => g.GameResourcesPage == baseGame)
				.Select(g => new GameNameModel
				{
					GameId = g.Id,
					DisplayName = g.DisplayName
				})
				.ToListAsync();
		}

		return View(gameList);
	}
}
