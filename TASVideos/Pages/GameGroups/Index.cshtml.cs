using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TASVideos.Data;
using TASVideos.Data.Entity.Game;
using TASVideos.Pages.Games.Groups.Models;

namespace TASVideos.Pages.GameGroups;

[AllowAnonymous]
public class IndexModel : BasePageModel
{
	private readonly ApplicationDbContext _db;

	[FromRoute]
	public int Id { get; set; }

	public IEnumerable<GameListEntry> Games { get; set; } = new List<GameListEntry>();

	public string Name { get; set; } = "";
	public string? Description { get; set; }

	public IndexModel(ApplicationDbContext db)
	{
		_db = db;
	}

	public async Task<IActionResult> OnGet()
	{
		var gameGroup = await _db.GameGroups.SingleOrDefaultAsync(gg => gg.Id == Id);

		if (gameGroup is null)
		{
			return NotFound();
		}

		Name = gameGroup.Name;
		Description = gameGroup.Description;

		Games = await _db.Games
			.ForGroup(Id)
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
