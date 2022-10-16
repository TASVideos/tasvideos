﻿using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TASVideos.Data;
using TASVideos.Data.Entity;
using TASVideos.WikiEngine;

namespace TASVideos.ViewComponents;

[WikiModule(WikiModules.GameSubPages)]
public class GameSubPages : ViewComponent
{
	private readonly ApplicationDbContext _db;

	public GameSubPages(ApplicationDbContext db)
	{
		_db = db;
	}

	public async Task<IViewComponentResult> InvokeAsync()
	{
		var model = await GetGameResourcesSubPages();
		return View(model);
	}

	private async Task<IEnumerable<GameSubpageModel>> GetGameResourcesSubPages()
	{
		// TODO: cache this
		var systems = await _db.GameSystems.ToListAsync();
		var gameResourceSystems = systems.Select(s => "GameResources/" + s.Code);

		var pages = _db.WikiPages
			.ThatAreNotDeleted()
			.WithNoChildren()
			.Where(wp => gameResourceSystems.Contains(wp.PageName))
			.Select(wp => wp.PageName)
			.ToList();

		return
			(from s in systems
			 join wp in pages on s.Code equals wp.Split('/').Last()
			 select new GameSubpageModel
			 {
				 SystemCode = s.Code,
				 SystemDescription = s.DisplayName,
				 PageLink = "GameResources/" + s.Code
			 })
			.ToList();
	}
}
