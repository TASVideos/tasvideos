using TASVideos.Core.Services.PublicationChain;
using TASVideos.Data.Entity.Game;

namespace TASVideos.Pages.Games;

[AllowAnonymous]
public class PublicationHistoryModel(ApplicationDbContext db, IPublicationHistory history) : BasePageModel
{
	[FromRoute]
	public int Id { get; set; }

	public PublicationHistoryGroup History { get; set; } = new();
	public Game Game { get; set; } = new();

	[FromQuery]
	public int? Highlight { get; set; }

	public async Task<IActionResult> OnGet()
	{
		var game = await db.Games.SingleOrDefaultAsync(p => p.Id == Id);

		if (game is null)
		{
			return NotFound();
		}

		Game = game;
		History = await history.ForGame(Id) ?? new PublicationHistoryGroup();
		return Page();
	}
}
