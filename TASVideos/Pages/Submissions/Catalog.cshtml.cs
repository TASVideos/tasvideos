using System.ComponentModel.DataAnnotations;
using System.Globalization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using TASVideos.Core.Services.ExternalMediaPublisher;
using TASVideos.Data;
using TASVideos.Data.Entity;
using TASVideos.Data.Entity.Game;

namespace TASVideos.Pages.Submissions;

[RequirePermission(PermissionTo.CatalogMovies)]
public class CatalogModel(ApplicationDbContext db, ExternalMediaPublisher publisher) : BasePageModel
{
	[FromRoute]
	public int Id { get; set; }

	[FromQuery]
	public int? GameId { get; set; }

	[FromQuery]
	public int? GameVersionId { get; set; }

	[BindProperty]
	public SubmissionCatalogModel Catalog { get; set; } = new();

	public List<SelectListItem> AvailableVersions { get; set; } = [];
	public List<SelectListItem> AvailableGames { get; set; } = [];
	public List<SelectListItem> AvailableSystems { get; set; } = [];
	public List<SelectListItem> AvailableSystemFrameRates { get; set; } = [];
	public List<SelectListItem> AvailableGoals { get; set; } = [];

	public async Task<IActionResult> OnGet()
	{
		var catalog = await db.Submissions
			.Where(s => s.Id == Id)
			.Select(s => new SubmissionCatalogModel
			{
				Title = s.Title,
				GameVersionId = s.GameVersionId,
				GameId = s.GameId,
				SystemId = s.SystemId,
				SystemFrameRateId = s.SystemFrameRateId,
				GameGoalId = s.GameGoalId
			})
			.SingleOrDefaultAsync();

		if (catalog is null)
		{
			return NotFound();
		}

		Catalog = catalog;
		if (GameId.HasValue)
		{
			var game = await db.Games.SingleOrDefaultAsync(g => g.Id == GameId);
			if (game is not null)
			{
				Catalog.GameId = game.Id;

				// We only want to pre-populate the Game Version if a valid Game was provided
				if (GameVersionId.HasValue)
				{
					var rom = await db.GameVersions.SingleOrDefaultAsync(r => r.GameId == game.Id && r.Id == GameVersionId && r.SystemId == Catalog.SystemId);
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

		var submission = await db.Submissions
			.IncludeTitleTables()
			.SingleOrDefaultAsync(s => s.Id == Id);
		if (submission is null)
		{
			return NotFound();
		}

		var externalMessages = new List<string>();

		if (submission.SystemId != Catalog.SystemId)
		{
			var system = await db.GameSystems.SingleOrDefaultAsync(s => s.Id == Catalog.SystemId!.Value);
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
			var systemFramerate = await db.GameSystemFrameRates.SingleOrDefaultAsync(s => s.Id == Catalog.SystemFrameRateId!.Value);
			if (systemFramerate is null)
			{
				ModelState.AddModelError($"{nameof(Catalog)}.{nameof(Catalog.SystemFrameRateId)}", $"Unknown System Framerate Id: {Catalog.SystemFrameRateId!.Value}");
			}
			else
			{
				externalMessages.Add($"Framerate changed from {(submission.SystemFrameRate?.FrameRate ?? 0.0).ToString(CultureInfo.InvariantCulture)} to {systemFramerate.FrameRate.ToString(CultureInfo.InvariantCulture)}");
				submission.SystemFrameRateId = Catalog.SystemFrameRateId!.Value;
				submission.SystemFrameRate = systemFramerate;
			}
		}

		if (submission.GameId != Catalog.GameId)
		{
			if (Catalog.GameId.HasValue)
			{
				var game = await db.Games.SingleOrDefaultAsync(s => s.Id == Catalog.GameId.Value);
				if (game is null)
				{
					ModelState.AddModelError($"{nameof(Catalog)}.{nameof(Catalog.GameId)}", $"Unknown Game Id: {Catalog.GameId.Value}");
				}
				else
				{
					externalMessages.Add($"Game changed from \"{submission.Game?.DisplayName}\" to \"{game.DisplayName}\"");
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

		if (submission.GameGoalId != Catalog.GameGoalId)
		{
			if (Catalog.GameGoalId.HasValue)
			{
				var gameGoal = await db.GameGoals.SingleOrDefaultAsync(gg => gg.Id == Catalog.GameGoalId);
				if (gameGoal is null)
				{
					ModelState.AddModelError($"{nameof(Catalog)}.{nameof(Catalog.GameGoalId)}", $"Unknown Game Goal Id: {Catalog.GameGoalId}");
				}
				else
				{
					externalMessages.Add($"Game Goal changed from \"{submission.GameGoal?.DisplayName}\" to \"{gameGoal.DisplayName}\"");
					submission.GameGoalId = Catalog.GameGoalId;
					submission.GameGoal = gameGoal;
				}
			}
			else
			{
				externalMessages.Add("Game Goal removed");
				submission.GameGoalId = null;
				submission.GameGoal = null;
			}
		}

		if (submission.GameVersionId != Catalog.GameVersionId)
		{
			if (Catalog.GameVersionId.HasValue)
			{
				var rom = await db.GameVersions.SingleOrDefaultAsync(s => s.Id == Catalog.GameVersionId.Value);
				if (rom is null)
				{
					ModelState.AddModelError($"{nameof(Catalog)}.{nameof(Catalog.GameVersionId)}", $"Unknown Game Version Id: {Catalog.GameVersionId.Value}");
				}
				else
				{
					externalMessages.Add($"Game Version changed from \"{submission.GameVersion?.Name}\" to \"{rom.Name}\"");
					submission.GameVersionId = Catalog.GameVersionId.Value;
					submission.GameVersion = rom;
				}
			}
			else
			{
				externalMessages.Add("Game Version removed");
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

		var result = await ConcurrentSave(db, $"{Id}S catalog updated", $"Unable to save {Id}S catalog");
		if (result && !Catalog.MinorEdit)
		{
			await publisher.SendGameManagement(
				$"{Id}S Catalog edited by {User.Name()}",
				$"[{Id}S]({{0}}) Catalog edited by {User.Name()}",
				$"{string.Join(", ", externalMessages)} | {submission.Title}",
				$"{Id}S");
		}

		return RedirectToPage("View", new { Id });
	}

	private async Task PopulateCatalogDropDowns()
	{
		if (!Catalog.SystemId.HasValue)
		{
			AvailableGames = await db.Games
				.OrderBy(g => g.DisplayName)
				.ToDropDown()
				.ToListAsync();

			AvailableVersions = await db.GameVersions
				.OrderBy(r => r.Name)
				.ToDropDown()
				.ToListAsync();
		}
		else
		{
			AvailableGames = await db.Games
				.ForSystem((int)Catalog.SystemId)
				.OrderBy(g => g.DisplayName)
				.ToDropDown()
				.ToListAsync();

			if (Catalog.GameId.HasValue)
			{
				AvailableVersions = await db.GameVersions
					.ForSystem((int)Catalog.SystemId)
					.ForGame((int)Catalog.GameId)
					.OrderBy(r => r.Name)
					.ToDropDown()
					.ToListAsync();
			}
			else
			{
				AvailableVersions = await db.GameVersions
					.ForSystem((int)Catalog.SystemId)
					.OrderBy(r => r.Name)
					.ToDropDown()
					.ToListAsync();
			}
		}

		AvailableSystems = await db.GameSystems
			.OrderBy(s => s.Code)
			.ToDropDownWithId()
			.ToListAsync();

		AvailableSystemFrameRates = Catalog.SystemId.HasValue
			? await db.GameSystemFrameRates
				.ForSystem(Catalog.SystemId.Value)
				.ToDropDown()
				.ToListAsync()
			: [];

		AvailableGoals = Catalog.GameId.HasValue
			? await db.GameGoals
				.Where(gg => gg.GameId == Catalog.GameId)
				.ToDropDown()
				.ToListAsync()
			: [];
	}

	public class SubmissionCatalogModel
	{
		public string Title { get; init; } = "";

		[Display(Name = "Game Version")]
		public int? GameVersionId { get; set; }

		[Display(Name = "Game")]
		public int? GameId { get; set; }

		[Display(Name = "System")]
		[Required]
		public int? SystemId { get; init; }

		[Display(Name = "System Framerate")]
		[Required]
		public int? SystemFrameRateId { get; init; }

		[Display(Name = "Goal")]
		public int? GameGoalId { get; init; }

		public bool MinorEdit { get; init; }
	}
}
