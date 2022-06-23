using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TASVideos.Data;
using TASVideos.Pages.UserFiles.Models;

namespace TASVideos.Pages.UserFiles;

[AllowAnonymous]
public class GameModel : BasePageModel
{
	private readonly ApplicationDbContext _db;

	public GameModel(ApplicationDbContext db)
	{
		_db = db;
	}

	public GameFileModel Game { get; set; } = new();

	[FromRoute]
	public int Id { get; set; }

	public async Task<IActionResult> OnGet()
	{
		var game = await _db.Games
			.Include(g => g.UserFiles)
			.ThenInclude(g => g.System)
			.Include(g => g.UserFiles)
			.ThenInclude(u => u.Author)
			.SingleOrDefaultAsync(g => g.Id == Id);

		if (game is null)
		{
			return NotFound();
		}

		Game = new GameFileModel
		{
			GameId = game.Id,
			GameName = game.DisplayName,
			Files = game.UserFiles
				.Where(uf => !uf.Hidden)
				.AsQueryable()
				.OrderByDescending(uf => uf.UploadTimestamp)
				.ToUserFileModel()
				.ToList()
		};

		return Page();
	}
}
