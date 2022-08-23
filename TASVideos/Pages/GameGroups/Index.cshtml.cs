using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TASVideos.Data;
using TASVideos.Data.Entity.Game;
using TASVideos.Pages.Games.Groups.Models;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;

namespace TASVideos.Pages.GameGroups;

[AllowAnonymous]
public class IndexModel : BasePageModel
{
	private readonly ApplicationDbContext _db;

	[FromRoute]
	public string Id { get; set; } = "";

	public int ParsedId => int.TryParse(Id, out var id) ? id : -1;

	public IEnumerable<GameListEntry> Games { get; set; } = new List<GameListEntry>();

	public string Name { get; set; } = "";
	public string? Description { get; set; }
	public string? Abbreviation { get; set; }

	public IndexModel(ApplicationDbContext db)
	{
		_db = db;
	}

	public async Task<IActionResult> OnGet()
	{
		var query = ParsedId > 0
			? _db.GameGroups.Where(g => g.Id == ParsedId)
			: _db.GameGroups.Where(g => g.Abbreviation == Id);

		// TODO: abbreviations need to be unique, then we can use Single here
		var gameGroup = await query
			.FirstOrDefaultAsync();

		if (gameGroup is null)
		{
			return NotFound();
		}

		Name = gameGroup.Name;
		Description = gameGroup.Description;
		Abbreviation = gameGroup.Abbreviation;

		Games = await _db.Games
			.ForGroup(gameGroup.Id)
			.Select(g => new GameListEntry
			{
				Id = g.Id,
				Name = g.DisplayName,
				PublicationCount = g.Publications.Count,
				SubmissionsCount = g.Submissions.Count,
				GameResourcesPage = g.GameResourcesPage
			})
			.OrderBy(g => g.Name)
			.ToListAsync();

		return Page();
	}
}
