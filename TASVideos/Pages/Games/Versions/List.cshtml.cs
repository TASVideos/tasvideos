using TASVideos.Data.Entity.Game;

namespace TASVideos.Pages.Games.Versions;

public class ListModel(ApplicationDbContext db) : BasePageModel
{
	[FromRoute]
	public int GameId { get; set; }

	public string GameDisplayName { get; set; } = "";

	public List<VersionEntry> Versions { get; set; } = [];

	public async Task<IActionResult> OnGet()
	{
		var displayName = await db.Games
			.Where(g => g.Id == GameId)
			.Select(g => g.DisplayName)
			.SingleOrDefaultAsync();

		if (displayName is null)
		{
			return NotFound();
		}

		GameDisplayName = displayName;
		Versions = await db.GameVersions
			.Where(v => v.GameId == GameId)
			.Select(v => new VersionEntry(
				v.Id,
				v.Name,
				v.Md5,
				v.Sha1,
				v.Version,
				v.Region,
				v.Type,
				v.System!.Code,
				v.TitleOverride,
				v.SourceDb))
			.ToListAsync();
		return Page();
	}

	public record VersionEntry(
		int Id,
		string Name,
		string? Md5,
		string? Sha1,
		string? Version,
		string? Region,
		VersionTypes Type,
		string System,
		string? TitleOverride,
		string? SourceDb);
}
