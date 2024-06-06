using TASVideos.Data.Entity.Game;

namespace TASVideos.Pages.Games.Versions;

[AllowAnonymous]
public class ViewModel(ApplicationDbContext db) : BasePageModel
{
	[FromRoute]
	public int GameId { get; set; }

	[FromRoute]
	public int Id { get; set; }

	[BindProperty]
	public VersionDisplay Version { get; set; } = null!;

	[BindProperty]
	public string Game { get; set; } = "";

	public async Task<IActionResult> OnGet()
	{
		var game = await db.Games
			.Where(g => g.Id == GameId)
			.Select(g => new { g.Id, g.DisplayName })
			.SingleOrDefaultAsync();

		if (game is null)
		{
			return NotFound();
		}

		Game = game.DisplayName;

		var version = await db.GameVersions
			.Where(r => r.Id == Id && r.Game!.Id == GameId)
			.Select(v => new VersionDisplay(
				v.System!.Code,
				v.Name,
				v.Md5,
				v.Sha1,
				v.Version,
				v.Region,
				v.Type,
				v.TitleOverride,
				game.Id,
				game.DisplayName))
			.SingleOrDefaultAsync();

		if (version is null)
		{
			return NotFound();
		}

		Version = version;

		return Page();
	}

	public record VersionDisplay(
		string SystemCode,
		string Name,
		string? Md5,
		string? Sha1,
		string? Version,
		string? Region,
		VersionTypes Type,
		string? TitleOverride,
		int GameId,
		string GameName);
}
