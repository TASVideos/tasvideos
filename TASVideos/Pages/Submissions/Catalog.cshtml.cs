using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using TASVideos.Core.Services.ExternalMediaPublisher;
using TASVideos.Data;
using TASVideos.Data.Entity;
using TASVideos.Data.Entity.Game;
using TASVideos.Pages.Submissions.Models;

namespace TASVideos.Pages.Submissions;

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
	public SubmissionCatalogModel Catalog { get; set; } = new();

	public IEnumerable<SelectListItem> AvailableRoms { get; set; } = new List<SelectListItem>();
	public IEnumerable<SelectListItem> AvailableGames { get; set; } = new List<SelectListItem>();
	public IEnumerable<SelectListItem> AvailableSystems { get; set; } = new List<SelectListItem>();
	public IEnumerable<SelectListItem> AvailableSystemFrameRates { get; set; } = new List<SelectListItem>();

	public async Task<IActionResult> OnGet()
	{
		var catalog = await _db.Submissions
			.Where(s => s.Id == Id)
			.Select(s => new SubmissionCatalogModel
			{
				Title = s.Title,
				GameVersionId = s.GameVersionId,
				GameId = s.GameId,
				SystemId = s.SystemId,
				SystemFrameRateId = s.SystemFrameRateId
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

				// We only want to pre-populate the Rom if a valid Game was provided
				if (GameVersionId.HasValue)
				{
					var rom = await _db.GameVersions.SingleOrDefaultAsync(r => r.GameId == game.Id && r.Id == GameVersionId && r.SystemId == Catalog.SystemId);
					if (rom is not null)
					{
						Catalog.GameVersionId = rom.Id;
					}
				}
			}
		}

		await PopulateCatalogDropDowns();
		return Page();
	}

	public async Task<IActionResult> OnPost()
	{
		if (!ModelState.IsValid)
		{
			await PopulateCatalogDropDowns();
			return Page();
		}

		var submission = await _db.Submissions
			.Include(s => s.System)
			.Include(s => s.SystemFrameRate)
			.Include(s => s.Game)
			.Include(s => s.GameVersion)
			.Include(s => s.SubmissionAuthors)
			.ThenInclude(sa => sa.Author)
			.SingleOrDefaultAsync(s => s.Id == Id);
		if (submission is null)
		{
			return NotFound();
		}

		var externalMessages = new List<string>();

		if (submission.SystemId != Catalog.SystemId)
		{
			var system = await _db.GameSystems.SingleOrDefaultAsync(s => s.Id == Catalog.SystemId!.Value);
			if (system is null)
			{
				ModelState.AddModelError($"{nameof(Catalog)}.{nameof(Catalog.SystemId)}", $"Unknown System Id: {Catalog.SystemId!.Value}");
			}
			else
			{
				externalMessages.Add($"System changed from {submission.System?.Code ?? ""} to {system.Code}");
				submission.SystemId = Catalog.SystemId!.Value;
				submission.System = system;
			}
		}

		if (submission.SystemFrameRateId != Catalog.SystemFrameRateId)
		{
			var systemFramerate = await _db.GameSystemFrameRates.SingleOrDefaultAsync(s => s.Id == Catalog.SystemFrameRateId!.Value);
			if (systemFramerate is null)
			{
				ModelState.AddModelError($"{nameof(Catalog)}.{nameof(Catalog.SystemFrameRateId)}", $"Unknown System Framerate Id: {Catalog.SystemFrameRateId!.Value}");
			}
			else
			{
				externalMessages.Add($"Framerate changed from {submission.SystemFrameRate?.FrameRate ?? 0.0} to {systemFramerate.FrameRate}");
				submission.SystemFrameRateId = Catalog.SystemFrameRateId!.Value;
				submission.SystemFrameRate = systemFramerate;
			}
		}

		if (submission.GameId != Catalog.GameId)
		{
			if (Catalog.GameId.HasValue)
			{
				var game = await _db.Games.SingleOrDefaultAsync(s => s.Id == Catalog.GameId.Value);
				if (game is null)
				{
					ModelState.AddModelError($"{nameof(Catalog)}.{nameof(Catalog.GameId)}", $"Unknown Game Id: {Catalog.GameId.Value}");
				}
				else
				{
					externalMessages.Add($"Game changed from {submission.Game?.DisplayName ?? "\"\""} to {game.DisplayName}");
					submission.GameId = Catalog.GameId.Value;
					submission.Game = game;
				}
			}
			else if (submission.GameId.HasValue)
			{
				externalMessages.Add("Game removed");
				submission.GameId = null;
				submission.Game = null;
			}
		}

		if (submission.GameVersionId != Catalog.GameVersionId)
		{
			if (Catalog.GameVersionId.HasValue)
			{
				var rom = await _db.GameVersions.SingleOrDefaultAsync(s => s.Id == Catalog.GameVersionId.Value);
				if (rom is null)
				{
					ModelState.AddModelError($"{nameof(Catalog)}.{nameof(Catalog.GameVersionId)}", $"Unknown Rom Id: {Catalog.GameVersionId.Value}");
				}
				else
				{
					externalMessages.Add($"Rom Hash changed from {submission.GameVersion?.Name ?? "\"\""} to {rom.Name}");
					submission.GameVersionId = Catalog.GameVersionId.Value;
					submission.GameVersion = rom;
				}
			}
			else
			{
				externalMessages.Add("Rom removed");
				submission.GameVersionId = null;
				submission.GameVersion = null;
			}
		}

		if (!ModelState.IsValid)
		{
			await PopulateCatalogDropDowns();
			return Page();
		}

		submission.GenerateTitle();

		var result = await ConcurrentSave(_db, $"{Id}S catalog updated", $"Unable to save {Id}S catalog");
		if (result)
		{
			await _publisher.SendSubmissionEdit(
				$"{Id}S Catalog edited by {User.Name()}",
				$"{string.Join(", ", externalMessages)} | {submission.Title}",
				$"{Id}S");
		}

		return RedirectToPage("View", new { Id });
	}

	private async Task PopulateCatalogDropDowns()
	{
		if (!Catalog.SystemId.HasValue)
		{
			AvailableGames = await _db.Games
				.OrderBy(g => g.DisplayName)
				.ToDropDown()
				.ToListAsync();

			AvailableRoms = await _db.GameVersions
				.OrderBy(r => r.Name)
				.Select(r => new SelectListItem
				{
					Value = r.Id.ToString(),
					Text = r.Name
				})
				.ToListAsync();
		}
		else
		{
			AvailableGames = await _db.Games
				.ForSystem((int)Catalog.SystemId)
				.OrderBy(g => g.DisplayName)
				.ToDropDown()
				.ToListAsync();

			if (Catalog.GameId.HasValue)
			{
				AvailableRoms = await _db.GameVersions
					.ForSystem((int)Catalog.SystemId)
					.ForGame((int)Catalog.GameId)
					.OrderBy(r => r.Name)
					.Select(r => new SelectListItem
					{
						Value = r.Id.ToString(),
						Text = r.Name
					})
					.ToListAsync();
			}
			else
			{
				AvailableRoms = await _db.GameVersions
					.ForSystem((int)Catalog.SystemId)
					.OrderBy(r => r.Name)
					.Select(r => new SelectListItem
					{
						Value = r.Id.ToString(),
						Text = r.Name
					})
					.ToListAsync();
			}
		}

		AvailableSystems = await _db.GameSystems
			.OrderBy(s => s.Code)
			.Select(s => new SelectListItem
			{
				Value = s.Id.ToString(),
				Text = s.Code
			})
			.ToListAsync();

		AvailableSystemFrameRates = Catalog.SystemId.HasValue
			? await _db.GameSystemFrameRates
				.ForSystem(Catalog.SystemId.Value)
				.ToDropDown()
				.ToListAsync()
			: new List<SelectListItem>();
	}
}
