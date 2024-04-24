using TASVideos.Data.Entity.Game;

namespace TASVideos.Pages.Games.Versions;

public class ListModel(ApplicationDbContext db) : BasePageModel
{
	[FromRoute]
	public int GameId { get; set; }

	public VersionList Versions { get; set; } = new();

	public async Task<IActionResult> OnGet()
	{
		var roms = await db.Games
			.Where(g => g.Id == GameId)
			.Select(g => new VersionList
			{
				GameDisplayName = g.DisplayName,
				Versions = g.GameVersions
				.Select(r => new VersionList.VersionEntry(
					r.Id,
					r.Name,
					r.Md5,
					r.Sha1,
					r.Version,
					r.Region,
					r.Type,
					r.System!.Code,
					r.TitleOverride))
				.ToList()
			})
			.SingleOrDefaultAsync();

		if (roms is null)
		{
			return NotFound();
		}

		Versions = roms;
		return Page();
	}

	public class VersionList
	{
		[Display(Name = "Game")]
		public string GameDisplayName { get; init; } = "";

		public List<VersionEntry> Versions { get; init; } = [];

		public record VersionEntry(
			int Id,
			string Name,
			string? Md5,
			string? Sha1,
			string? Version,
			string? Region,
			VersionTypes Type,
			string System,
			string? TitleOverride);
	}
}
