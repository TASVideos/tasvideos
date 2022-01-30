using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TASVideos.Core.Services;
using TASVideos.Data;
using TASVideos.Data.Entity;
using TASVideos.WikiEngine;

namespace TASVideos.ViewComponents;

[WikiModule(WikiModules.PlayerPointsTable)]
public class PlayerPointsTable : ViewComponent
{
	private readonly ApplicationDbContext _db;
	private readonly IPointsService _pointsService;

	public PlayerPointsTable(ApplicationDbContext db, IPointsService pointsService)
	{
		_db = db;
		_pointsService = pointsService;
	}

	public async Task<IViewComponentResult> InvokeAsync(int? count)
	{
		var showCount = count ?? 50;

		var players = await _db.Users
			.ThatArePublishedAuthors()
			.Select(u => new PlayerPointsModel
			{
				Id = u.Id,
				Player = u.UserName
			})
			.ToListAsync();

		foreach (var user in players)
		{
			user.Points = await _pointsService.PlayerPoints(user.Id);
		}

		var sortedPlayers = players.OrderByDescending(u => u.Points).Take(showCount);

		int counter = 0;
		foreach (var user in sortedPlayers)
		{
			user.Position = ++counter;
		}

		return View(sortedPlayers);
	}
}
