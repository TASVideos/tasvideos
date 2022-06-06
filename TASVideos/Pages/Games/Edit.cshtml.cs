﻿using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using TASVideos.Core.Services;
using TASVideos.Data;
using TASVideos.Data.Entity;
using TASVideos.Data.Entity.Game;
using TASVideos.Pages.Games.Models;

namespace TASVideos.Pages.Games;

[RequirePermission(PermissionTo.CatalogMovies)]
public class EditModel : BasePageModel
{
	private readonly ApplicationDbContext _db;
	private readonly IWikiPages _wikiPages;

	public EditModel(
		ApplicationDbContext db,
		IWikiPages wikiPages)
	{
		_db = db;
		_wikiPages = wikiPages;
	}

	[FromRoute]
	public int? Id { get; set; }

	[BindProperty]
	public GameEditModel Game { get; set; } = new();

	public bool CanDelete { get; set; }

	[Display(Name = "Available Genres")]
	public IEnumerable<SelectListItem> AvailableGenres { get; set; } = new List<SelectListItem>();

	[Display(Name = "Available Groups")]
	public IEnumerable<SelectListItem> AvailableGroups { get; set; } = new List<SelectListItem>();

	public async Task<IActionResult> OnGet()
	{
		if (Id.HasValue)
		{
			var game = await _db.Games
				.Where(g => g.Id == Id)
				.Select(g => new GameEditModel
				{
					GoodName = g.GoodName,
					DisplayName = g.DisplayName,
					Abbreviation = g.Abbreviation,
					SearchKey = g.SearchKey,
					YoutubeTags = g.YoutubeTags,
					ScreenshotUrl = g.ScreenshotUrl,
					GameResourcesPage = g.GameResourcesPage,
					Genres = g.GameGenres.Select(gg => gg.GenreId),
					Groups = g.GameGroups.Select(gg => gg.GameGroupId)
				})
				.SingleOrDefaultAsync();

			if (game is null)
			{
				return NotFound();
			}

			Game = game;
		}

		await Initialize();
		return Page();
	}

	public async Task<IActionResult> OnPost()
	{
		if (!ModelState.IsValid)
		{
			await Initialize();
			return Page();
		}

		if (!string.IsNullOrEmpty(Game.GameResourcesPage))
		{
			var page = await _wikiPages.Page(Game.GameResourcesPage);
			if (page is null)
			{
				ModelState.AddModelError($"{nameof(Game)}.{nameof(Game.GameResourcesPage)}", $"Page {Game.GameResourcesPage} not found");
				await Initialize();
				return Page();
			}
		}

		Game? game;
		if (Id.HasValue)
		{
			game = await _db.Games
				.Include(g => g.GameGenres)
				.Include(g => g.GameGroups)
				.SingleOrDefaultAsync(g => g.Id == Id.Value);
			if (game is null)
			{
				return NotFound();
			}

			game.GoodName = Game.GoodName;
			game.DisplayName = Game.DisplayName;
			game.Abbreviation = Game.Abbreviation;
			game.SearchKey = Game.SearchKey;
			game.YoutubeTags = Game.YoutubeTags;
			game.ScreenshotUrl = Game.ScreenshotUrl;
			game.GameResourcesPage = Game.GameResourcesPage;
			SetGameValues(game, Game);
			await ConcurrentSave(_db, $"Game {Id} updated", $"Unable to update Game {Id}");
		}
		else
		{
			game = new Game
			{
				GoodName = Game.GoodName,
				DisplayName = Game.DisplayName,
				Abbreviation = Game.Abbreviation,
				SearchKey = Game.SearchKey,
				YoutubeTags = Game.YoutubeTags,
				ScreenshotUrl = Game.ScreenshotUrl,
				GameResourcesPage = Game.GameResourcesPage
			};
			_db.Games.Add(game);
			SetGameValues(game, Game);
			await ConcurrentSave(_db, $"Game {game.GoodName} created", "Unable to create game");
		}

		return BasePageRedirect("Index", new { game.Id });
	}

	private static void SetGameValues(Game game, GameEditModel editModel)
	{
		game.GameResourcesPage = editModel.GameResourcesPage;
		game.GameGenres.SetGenres(editModel.Genres);
		game.GameGroups.SetGroups(editModel.Groups);
	}

	public async Task<IActionResult> OnPostDelete()
	{
		if (!Id.HasValue)
		{
			return NotFound();
		}

		if (!await CanBeDeleted())
		{
			ErrorStatusMessage($"Unable to delete Game {Id}, game is used by a publication or submission.");
			return BasePageRedirect("List");
		}

		_db.Games.Attach(new Game { Id = Id ?? 0 }).State = EntityState.Deleted;
		await ConcurrentSave(_db, $"Game {Id} deleted", $"Unable to delete Game {Id}");

		return BasePageRedirect("List");
	}

	private async Task Initialize()
	{
		AvailableGenres = await _db.Genres
			.OrderBy(g => g.DisplayName)
			.ToDropdown()
			.ToListAsync();

		AvailableGroups = await _db.GameGroups
			.OrderBy(g => g.Name)
			.ToDropdown()
			.ToListAsync();

		CanDelete = await CanBeDeleted();
	}

	private async Task<bool> CanBeDeleted()
	{
		return Id > 0
			&& !await _db.Submissions.AnyAsync(s => s.Game!.Id == Id)
			&& !await _db.Publications.AnyAsync(p => p.Game!.Id == Id);
	}
}
