using TASVideos.Data.Entity.Game;

namespace TASVideos.Pages.GameGroups;

[AllowAnonymous]
public class IndexModel(ApplicationDbContext db) : BasePageModel
{
	[FromRoute]
	public string Id { get; set; } = "";

	public int ParsedId => int.TryParse(Id, out var id) ? id : -1;

	public List<GameEntry> Games { get; set; } = [];

	public string Name { get; set; } = "";
	public string? Description { get; set; }
	public string? Abbreviation { get; set; }

	public async Task<IActionResult> OnGet()
	{
		var query = ParsedId > 0
			? db.GameGroups.Where(g => g.Id == ParsedId)
			: db.GameGroups.Where(g => g.Abbreviation == Id);

		var gameGroup = await query.SingleOrDefaultAsync();
		if (gameGroup is null)
		{
			return NotFound();
		}

		Name = gameGroup.Name;
		Description = gameGroup.Description;
		Abbreviation = gameGroup.Abbreviation;

		Games = await db.Games
			.ForGroup(gameGroup.Id)
			.Select(g => new GameEntry(
				g.Id,
				g.DisplayName,
				g.GameVersions
					.Select(v => v.System!.Code)
					.Distinct()
					.OrderBy(s => s)
					.ToList(),
				g.Publications.Count,
				g.Submissions.Count,
				g.GameResourcesPage))
			.ToListAsync();

		return Page();
	}

	public record GameEntry(int Id, string Name, List<string> Systems, int PubCount, int SubCount, string? GameResourcesPage)
	{
		public string SystemsString() => string.Join(", ", Systems);
	}
}
