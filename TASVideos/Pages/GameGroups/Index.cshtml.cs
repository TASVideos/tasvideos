﻿using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TASVideos.Data;
using TASVideos.Data.Entity.Game;
using TASVideos.Pages.Games.Groups.Models;

namespace TASVideos.Pages.GameGroups;

[AllowAnonymous]
public class IndexModel(ApplicationDbContext db) : BasePageModel
{
	[FromRoute]
	public string Id { get; set; } = "";

	public int ParsedId => int.TryParse(Id, out var id) ? id : -1;

	public IEnumerable<GameListEntry> Games { get; set; } = new List<GameListEntry>();

	public string Name { get; set; } = "";
	public string? Description { get; set; }
	public string? Abbreviation { get; set; }

	public async Task<IActionResult> OnGet()
	{
		var query = ParsedId > 0
			? db.GameGroups.Where(g => g.Id == ParsedId)
			: db.GameGroups.Where(g => g.Abbreviation == Id);

		var gameGroup = await query
			.SingleOrDefaultAsync();

		if (gameGroup is null)
		{
			return NotFound();
		}

		Name = gameGroup.Name;
		Description = gameGroup.Description;
		Abbreviation = gameGroup.Abbreviation;

		Games = await db.Games
			.ForGroup(gameGroup.Id)
			.Select(g => new GameListEntry
			{
				Id = g.Id,
				Name = g.DisplayName,
				Systems = g.GameVersions
					.Select(v => v.System!.Code)
					.Distinct()
					.OrderBy(s => s)
					.ToList(),
				PublicationCount = g.Publications.Count,
				SubmissionsCount = g.Submissions.Count,
				GameResourcesPage = g.GameResourcesPage
			})
			.ToListAsync();

		return Page();
	}
}
