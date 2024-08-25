using TASVideos.Data.Entity.Game;

namespace TASVideos.Pages.Games.Versions;

[AllowAnonymous]
public class ViewModel(ApplicationDbContext db) : BasePageModel
{
	[FromRoute]
	public int GameId { get; set; }

	[FromRoute]
	public int Id { get; set; }

	public string Game { get; set; } = "";
	public VersionDisplay Version { get; set; } = null!;
	public List<Entry> Publications { get; set; } = [];
	public List<Entry> Submissions { get; set; } = [];

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
			.Where(v => v.Id == Id && v.Game!.Id == GameId)
			.Select(v => new VersionDisplay(
				v.Id,
				v.System!.Code,
				v.Name,
				v.Md5,
				v.Sha1,
				v.Version,
				v.Region,
				v.Type,
				v.TitleOverride,
				v.SourceDb,
				v.Notes))
			.SingleOrDefaultAsync();

		if (version is null)
		{
			return NotFound();
		}

		Version = version;

		Publications = await db.Publications
			.Where(p => p.GameVersionId == version.Id)
			.Select(p => new Entry(p.Id, p.Title))
			.ToListAsync();

		Submissions = await db.Submissions
			.Where(p => p.GameVersionId == version.Id)
			.Select(p => new Entry(p.Id, p.Title))
			.ToListAsync();

		return Page();
	}

	public record VersionDisplay(
		int Id,
		string SystemCode,
		string Name,
		string? Md5,
		string? Sha1,
		string? Version,
		string? Region,
		VersionTypes Type,
		string? TitleOverride,
		string? SourceDb,
		string? Notes);

	public record Entry(int Id, string Title);
}
