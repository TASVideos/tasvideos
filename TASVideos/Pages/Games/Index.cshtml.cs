using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TASVideos.Data;
using TASVideos.Pages.Games.Models;
using TASVideos.ViewComponents;

namespace TASVideos.Pages.Games;

[AllowAnonymous]
public class IndexModel : BasePageModel
{
	private readonly ApplicationDbContext _db;

	public IndexModel(ApplicationDbContext db)
	{
		_db = db;
	}

	[FromRoute]
	public int Id { get; set; }

	public GameDisplayModel Game { get; set; } = new();

	public IEnumerable<MiniMovieModel> Movies { get; set; } = new List<MiniMovieModel>();

	public async Task<IActionResult> OnGet()
	{
		var game = await _db.Games
			.ToGameDisplayModel()
			.SingleOrDefaultAsync(g => g.Id == Id);

		if (game is null)
		{
			return NotFound();
		}

		Game = game;
		Movies = await _db.Publications
			.Where(p => p.GameId == Id && p.ObsoletedById == null)
			.OrderBy(p => p.Branch == null ? -1 : p.Branch.Length)
			.ThenBy(p => p.Frames)
			.ToMiniMovieModel()
			.ToListAsync();

		return Page();
	}
}
