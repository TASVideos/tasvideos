using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ViewComponents;
using Microsoft.EntityFrameworkCore;
using TASVideos.Data;
using TASVideos.WikiEngine;

namespace TASVideos.ViewComponents;

[WikiModule(WikiModules.DisplayGameName)]
public class DisplayGameName : ViewComponent
{
	private readonly ApplicationDbContext _db;

	public DisplayGameName(ApplicationDbContext db)
	{
		_db = db;
	}

	public async Task<IViewComponentResult> InvokeAsync(IList<int> gid)
	{
		if (!gid.Any())
		{
			return new ContentViewComponentResult("<<< No gamename ID specified >>>");
		}

		var games = await _db.Games
			.Where(g => gid.Contains(g.Id))
			.Include(g => g.System)
			.OrderBy(d => d)
			.ToListAsync();

		var displayNames = games
			.OrderBy(g => g.System!.Code)
			.ThenBy(g => g.DisplayName)
			.Select(g => $"{g.System!.Code} {g.DisplayName}");

		return new ContentViewComponentResult(string.Join(", ", displayNames));
	}
}
