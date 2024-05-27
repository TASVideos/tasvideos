using System.Globalization;
using Microsoft.AspNetCore.Mvc.Rendering;
using TASVideos.Core.Services.ExternalMediaPublisher;

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
	public SubmissionCatalog Catalog { get; set; } = new();

	public List<SelectListItem> AvailableVersions { get; set; } = [];
	public List<SelectListItem> AvailableGames { get; set; } = [];
	public List<SelectListItem> AvailableSystems { get; set; } = [];
	public List<SelectListItem> AvailableSystemFrameRates { get; set; } = [];
	public List<SelectListItem> AvailableGoals { get; set; } = [];

	public async Task<IActionResult> OnGet()
	{
		var catalog = await db.Submissions
			.Where(s => s.Id == Id)
			.Select(s => new SubmissionCatalog
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
			var game = await db.Games.FindAsync(GameId);
			if (game is not null)
			{
				Catalog.GameId = game.Id;

				// We only want to pre-populate the Game Version if a valid Game was provided
				if (GameVersionId.HasValue)
				{
					var version = await db.GameVersions.SingleOrDefaultAsync(r => r.GameId == game.Id && r.Id == GameVersionId && r.SystemId == Catalog.SystemId);
					if (version is not null)
					{
						Catalog.GameVersionId = version.Id;
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
			.Include(s => s.SubmissionAuthors)
			.ThenInclude(sa => sa.Author)
			.Include(s => s.System)
			.Include(s => s.SystemFrameRate)
			.Include(s => s.Game)
			.Include(s => s.GameVersion)
			.Include(s => s.GameGoal)
			.Include(gg => gg.GameGoal)
			.SingleOrDefaultAsync(s => s.Id == Id);
		if (submission is null)
		{
			return NotFound();
		}

		var externalMessages = new List<string>();

		if (submission.SystemId != Catalog.SystemId)
		{
			var system = await db.GameSystems.FindAsync(Catalog.SystemId!.Value);
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
			var systemFramerate = await db.GameSystemFrameRates.FindAsync(Catalog.SystemFrameRateId!.Value);
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
				var game = await db.Games.FindAsync(Catalog.GameId.Value);
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
				var gameGoal = await db.GameGoals.FindAsync(Catalog.GameGoalId);
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
				var version = await db.GameVersions.FindAsync(Catalog.GameVersionId.Value);
				if (version is null)
				{
					ModelState.AddModelError($"{nameof(Catalog)}.{nameof(Catalog.GameVersionId)}", $"Unknown Game Version Id: {Catalog.GameVersionId.Value}");
				}
				else
				{
					externalMessages.Add($"Game Version changed from \"{submission.GameVersion?.Name}\" to \"{version.Name}\"");
					submission.GameVersionId = Catalog.GameVersionId.Value;
					submission.GameVersion = version;
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

		var result = await db.TrySaveChanges();
		SetMessage(result, $"{Id}S catalog updated", $"Unable to save {Id}S catalog");
		if (result.IsSuccess() && !Catalog.MinorEdit)
		{
			await publisher.SendGameManagement(
				$"[{Id}S]({{0}}) Catalog edited by {User.Name()}",
				$"{string.Join(", ", externalMessages)} | {submission.Title}",
				$"{Id}S");
		}

		return RedirectToPage("View", new { Id });
	}

	private async Task PopulateCatalogDropDowns()
	{
		AvailableGames = await db.Games.ToDropDownList(Catalog.SystemId);
		AvailableVersions = await db.GameVersions.ToDropDownList(Catalog.SystemId, Catalog.GameId);
		AvailableSystems = await db.GameSystems.ToDropDownListWithId();
		AvailableSystemFrameRates = Catalog.SystemId.HasValue
			? await db.GameSystemFrameRates.ToDropDownList(Catalog.SystemId.Value)
			: [];

		AvailableGoals = Catalog.GameId.HasValue
			? await db.GameGoals.ToDropDownList(Catalog.GameId.Value)
			: [];
	}

	public class SubmissionCatalog
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
