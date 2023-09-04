﻿using System.Globalization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using TASVideos.Core.Services.ExternalMediaPublisher;
using TASVideos.Data;
using TASVideos.Data.Entity;
using TASVideos.Data.Entity.Game;
using TASVideos.Pages.Publications.Models;

namespace TASVideos.Pages.Publications;

[RequirePermission(PermissionTo.CatalogMovies)]
public class CatalogModel : BasePageModel
{
	private readonly ApplicationDbContext _db;
	private readonly ExternalMediaPublisher _publisher;

	public CatalogModel(
		ApplicationDbContext db,
		ExternalMediaPublisher publisher)
	{
		_db = db;
		_publisher = publisher;
	}

	[FromRoute]
	public int Id { get; set; }

	[FromQuery]
	public int? GameId { get; set; }

	[FromQuery]
	public int? GameVersionId { get; set; }

	[BindProperty]
	public PublicationCatalogModel Catalog { get; set; } = new();

	public IEnumerable<SelectListItem> AvailableVersions { get; set; } = new List<SelectListItem>();
	public IEnumerable<SelectListItem> AvailableGames { get; set; } = new List<SelectListItem>();
	public IEnumerable<SelectListItem> AvailableSystems { get; set; } = new List<SelectListItem>();
	public IEnumerable<SelectListItem> AvailableSystemFrameRates { get; set; } = new List<SelectListItem>();

	public async Task<IActionResult> OnGet()
	{
		var catalog = await _db.Publications
				.Where(p => p.Id == Id)
				.Select(p => new PublicationCatalogModel
				{
					Title = p.Title,
					GameVersionId = p.GameVersionId,
					GameId = p.GameId,
					SystemId = p.SystemId,
					SystemFrameRateId = p.SystemFrameRateId
				})
				.SingleOrDefaultAsync();

		if (catalog is null)
		{
			return NotFound();
		}

		Catalog = catalog;
		if (GameId.HasValue)
		{
			var game = await _db.Games.SingleOrDefaultAsync(g => g.Id == GameId);
			if (game is not null)
			{
				Catalog.GameId = game.Id;

				// We only want to pre-populate the Game Version if a valid Game was provided
				if (GameVersionId.HasValue)
				{
					var gameVersion = await _db.GameVersions.SingleOrDefaultAsync(r => r.GameId == game.Id && r.Id == GameVersionId && r.SystemId == Catalog.SystemId);
					if (gameVersion is not null)
					{
						Catalog.GameVersionId = gameVersion.Id;
					}
				}
			}
		}

		await PopulateCatalogDropDowns(Catalog.GameId, Catalog.SystemId);

		return Page();
	}

	public async Task<IActionResult> OnPost()
	{
		if (!ModelState.IsValid)
		{
			await PopulateCatalogDropDowns(Catalog.GameId, Catalog.SystemId);
			return Page();
		}

		var publication = await _db.Publications
			.IncludeTitleTables()
			.SingleOrDefaultAsync(s => s.Id == Id);
		if (publication is null)
		{
			return NotFound();
		}

		var externalMessages = new List<string>();

		if (publication.SystemId != Catalog.SystemId)
		{
			var system = await _db.GameSystems.SingleOrDefaultAsync(s => s.Id == Catalog.SystemId);
			if (system is null)
			{
				ModelState.AddModelError($"{nameof(Catalog)}.{nameof(Catalog.SystemId)}", $"Unknown System Id: {Catalog.SystemId}");
			}
			else
			{
				externalMessages.Add($"System changed from {publication.System!.Code} to {system.Code}");
				publication.SystemId = Catalog.SystemId;
				publication.System = system;
			}
		}

		if (publication.SystemFrameRateId != Catalog.SystemFrameRateId)
		{
			var systemFramerate = await _db.GameSystemFrameRates.SingleOrDefaultAsync(s => s.Id == Catalog.SystemFrameRateId);
			if (systemFramerate is null)
			{
				ModelState.AddModelError($"{nameof(Catalog)}.{nameof(Catalog.SystemFrameRateId)}", $"Unknown System Id: {Catalog.SystemFrameRateId}");
			}
			else if (publication.SystemFrameRateId != Catalog.SystemFrameRateId)
			{
				externalMessages.Add($"Framerate changed from {publication.SystemFrameRate!.FrameRate.ToString(CultureInfo.InvariantCulture)} to {systemFramerate.FrameRate.ToString(CultureInfo.InvariantCulture)}");
				publication.SystemFrameRateId = Catalog.SystemFrameRateId;
				publication.SystemFrameRate = systemFramerate;
			}
		}

		if (publication.GameId != Catalog.GameId)
		{
			var game = await _db.Games.SingleOrDefaultAsync(s => s.Id == Catalog.GameId);
			if (game is null)
			{
				ModelState.AddModelError($"{nameof(Catalog)}.{nameof(Catalog.GameId)}", $"Unknown System Id: {Catalog.GameId}");
			}
			else
			{
				externalMessages.Add($"Game changed from {publication.Game!.DisplayName} to {game.DisplayName}");
				publication.GameId = Catalog.GameId;
				publication.Game = game;
			}
		}

		if (publication.GameVersionId != Catalog.GameVersionId)
		{
			var gameVersion = await _db.GameVersions.SingleOrDefaultAsync(s => s.Id == Catalog.GameVersionId);
			if (gameVersion is null)
			{
				ModelState.AddModelError($"{nameof(Catalog)}.{nameof(Catalog.GameVersionId)}", $"Unknown System Id: {Catalog.GameVersionId}");
			}
			else
			{
				externalMessages.Add($"Game Version changed from {publication.GameVersion!.Name} to {gameVersion.Name}");
				publication.GameVersionId = Catalog.GameVersionId;
				publication.GameVersion = gameVersion;
			}

			if (!ModelState.IsValid)
			{
				await PopulateCatalogDropDowns(Catalog.GameId, Catalog.SystemId);
				return Page();
			}
		}

		publication.GenerateTitle();

		var result = await ConcurrentSave(_db, $"{Id}M catalog updated", $"Unable to save {Id}M catalog");
		if (result && !Catalog.MinorEdit)
		{
			await _publisher.SendGameManagement(
				$"{Id}M Catalog edited by {User.Name()}",
				$"[{Id}M]({{0}}) Catalog edited by {User.Name()}",
				$"{string.Join(", ", externalMessages)} | {publication.Title}",
				$"{Id}M");
		}

		return RedirectToPage("View", new { Id });
	}

	private async Task PopulateCatalogDropDowns(int gameId, int systemId)
	{
		AvailableVersions = UiDefaults.DefaultEntry.Concat(await _db.GameVersions
			.ForGame(gameId)
			.ForSystem(systemId)
			.OrderBy(r => r.Name)
			.Select(r => new SelectListItem
			{
				Value = r.Id.ToString(),
				Text = r.Name
			})
			.ToListAsync());

		AvailableGames = await _db.Games
			.ForSystem(systemId)
			.OrderBy(g => g.DisplayName)
			.Select(g => new SelectListItem
			{
				Value = g.Id.ToString(),
				Text = g.DisplayName
			})
			.ToListAsync();

		AvailableSystems = await _db.GameSystems
			.OrderBy(s => s.Code)
			.Select(s => new SelectListItem
			{
				Value = s.Id.ToString(),
				Text = s.Code
			})
			.ToListAsync();

		AvailableSystemFrameRates = await _db.GameSystemFrameRates
			.ForSystem(systemId)
			.ToDropDown()
			.ToListAsync();
	}
}
