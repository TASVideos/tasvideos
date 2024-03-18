using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TASVideos.Data;
using TASVideos.Data.Entity;
using TASVideos.Pages.UserFiles.Models;

namespace TASVideos.Pages.UserFiles;

[AllowAnonymous]
public class GameModel(ApplicationDbContext db) : BasePageModel
{
	public GameFileModel Game { get; set; } = new();

	[FromRoute]
	public int Id { get; set; }

	public async Task<IActionResult> OnGet()
	{
		var game = await db.Games.SingleOrDefaultAsync(g => g.Id == Id);
		if (game is null)
		{
			return NotFound();
		}

		Game = new GameFileModel
		{
			GameId = game.Id,
			GameName = game.DisplayName,
			Files = db.UserFiles
				.ForGame(game.Id)
				.HideIfNotAuthor(User.GetUserId())
				.AsQueryable()
				.OrderByDescending(uf => uf.UploadTimestamp)
				.ToUserFileModel()
				.ToList()
		};

		return Page();
	}
}
