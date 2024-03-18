﻿using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ViewComponents;
using TASVideos.Data;
using TASVideos.WikiEngine;

namespace TASVideos.ViewComponents;

[WikiModule(WikiModules.DisplayGameName)]
public class DisplayGameName(ApplicationDbContext db) : ViewComponent
{
	public async Task<IViewComponentResult> InvokeAsync(IList<int> gid)
	{
		if (!gid.Any())
		{
			return new ContentViewComponentResult("<<< No gamename ID specified >>>");
		}

		var games = await db.Games
			.Where(g => gid.Contains(g.Id))
			.OrderBy(d => d)
			.ToListAsync();

		var displayNames = games
			.OrderBy(g => g.DisplayName)
			.Select(g => $"{g.DisplayName}");

		return new ContentViewComponentResult(string.Join(", ", displayNames));
	}
}
