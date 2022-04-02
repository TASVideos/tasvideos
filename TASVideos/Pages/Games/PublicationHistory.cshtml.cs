using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TASVideos.Core.Services;
using TASVideos.Core.Services.PublicationChain;
using TASVideos.Data;
using TASVideos.Data.Entity.Game;

namespace TASVideos.Pages.Games;

[AllowAnonymous]
public class PublicationHistoryModel : BasePageModel
{
	private readonly ApplicationDbContext _db;
	private readonly IPublicationHistory _history;

	public PublicationHistoryModel(
		ApplicationDbContext db,
		IPublicationHistory history)
	{
		_history = history;
		_db = db;
	}

	[FromRoute]
	public int Id { get; set; }

	public PublicationHistoryGroup History { get; set; } = new();
	public Game Game { get; set; } = new();

	[FromQuery]
	public int? Highlight { get; set; }

	public async Task<IActionResult> OnGet()
	{
		var game = await _db.Games.SingleOrDefaultAsync(p => p.Id == Id);

		if (game is null)
		{
			return NotFound();
		}

		Game = game;
		History = await _history.ForGame(Id) ?? new PublicationHistoryGroup();
		return Page();
	}
}
