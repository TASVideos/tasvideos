using TASVideos.Models;

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
			Files = await db.UserFiles
				.ForGame(game.Id)
				.HideIfNotAuthor(User.GetUserId())
				.AsQueryable()
				.OrderByDescending(uf => uf.UploadTimestamp)
				.ToUserFileModel()
				.ToListAsync()
		};

		return Page();
	}

	public class GameFileModel
	{
		public int GameId { get; init; }
		public string GameName { get; init; } = "";

		public List<UserFileModel> Files { get; init; } = [];
	}
}
