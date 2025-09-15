using System.Globalization;

namespace TASVideos.Pages.Submissions;

[RequirePermission(PermissionTo.CatalogMovies)]
public class CatalogModel(ApplicationDbContext db, IExternalMediaPublisher publisher) : BasePageModel
{
	[FromRoute]
	public int Id { get; set; }

	[FromQuery]
	public int? SystemId { get; set; }

	[FromQuery]
	public int? GameId { get; set; }

	[FromQuery]
	public int? GameVersionId { get; set; }

	[FromQuery]
	public int? GameGoalId { get; set; }

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
				GameVersion = s.GameVersionId,
				Game = s.GameId,
				System = s.SystemId,
				SystemFramerate = s.SystemFrameRateId,
				Goal = s.GameGoalId,
				Emulator = s.EmulatorVersion,
				SyncVerified = s.SyncedOn.HasValue,
				SyncVerifiedOn = s.SyncedOn,
				SyncedBy = s.SyncedByUser != null ? s.SyncedByUser.UserName : null,
				AdditionalSyncNotes = s.AdditionalSyncNotes
			})
			.SingleOrDefaultAsync();

		if (catalog is null)
		{
			return NotFound();
		}

		Catalog = catalog;

		if (SystemId.HasValue)
		{
			var system = await db.GameSystems.FindAsync(SystemId);
			if (system is not null)
			{
				Catalog.System = system.Id;
			}
		}

		if (GameId.HasValue)
		{
			var game = await db.Games.FindAsync(GameId);
			if (game is not null)
			{
				Catalog.Game = game.Id;

				// We only want to pre-populate the Game Version if a valid Game was provided
				if (GameVersionId.HasValue)
				{
					var version = await db.GameVersions.SingleOrDefaultAsync(r => r.GameId == game.Id && r.Id == GameVersionId && r.SystemId == Catalog.System);
					if (version is not null)
					{
						Catalog.GameVersion = version.Id;
					}
				}

				if (GameGoalId.HasValue)
				{
					var gameGoal = await db.GameGoals.SingleOrDefaultAsync(gg => gg.GameId == game.Id && gg.Id == GameGoalId);
					if (gameGoal is not null)
					{
						Catalog.Goal = gameGoal.Id;
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

		if (submission.SystemId != Catalog.System)
		{
			var system = await db.GameSystems.FindAsync(Catalog.System!.Value);
			if (system is null)
			{
				ModelState.AddModelError($"{nameof(Catalog)}.{nameof(Catalog.System)}", $"Unknown System Id: {Catalog.System!.Value}");
			}
			else
			{
				externalMessages.Add($"System changed from {submission.System?.Code ?? ""} to {system.Code}");
				submission.SystemId = Catalog.System!.Value;
				submission.System = system;
			}
		}

		if (submission.SystemFrameRateId != Catalog.SystemFramerate)
		{
			var systemFramerate = await db.GameSystemFrameRates.FindAsync(Catalog.SystemFramerate!.Value);
			if (systemFramerate is null)
			{
				ModelState.AddModelError($"{nameof(Catalog)}.{nameof(Catalog.SystemFramerate)}", $"Unknown System Framerate Id: {Catalog.SystemFramerate!.Value}");
			}
			else
			{
				externalMessages.Add($"Framerate changed from {(submission.SystemFrameRate?.FrameRate ?? 0.0).ToString(CultureInfo.InvariantCulture)} to {systemFramerate.FrameRate.ToString(CultureInfo.InvariantCulture)}");
				submission.SystemFrameRateId = Catalog.SystemFramerate!.Value;
				submission.SystemFrameRate = systemFramerate;
			}
		}

		if (submission.GameId != Catalog.Game)
		{
			if (Catalog.Game.HasValue)
			{
				var game = await db.Games.FindAsync(Catalog.Game.Value);
				if (game is null)
				{
					ModelState.AddModelError($"{nameof(Catalog)}.{nameof(Catalog.Game)}", $"Unknown Game Id: {Catalog.Game.Value}");
				}
				else
				{
					externalMessages.Add($"Game changed from \"{submission.Game?.DisplayName}\" to \"{game.DisplayName}\"");
					submission.GameId = Catalog.Game.Value;
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

		if (submission.GameGoalId != Catalog.Goal)
		{
			if (Catalog.Goal.HasValue)
			{
				var gameGoal = await db.GameGoals.FindAsync(Catalog.Goal);
				if (gameGoal is null)
				{
					ModelState.AddModelError($"{nameof(Catalog)}.{nameof(Catalog.Goal)}", $"Unknown Game Goal Id: {Catalog.Goal}");
				}
				else
				{
					externalMessages.Add($"Game Goal changed from \"{submission.GameGoal?.DisplayName}\" to \"{gameGoal.DisplayName}\"");
					submission.GameGoalId = Catalog.Goal;
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

		if (submission.GameVersionId != Catalog.GameVersion)
		{
			if (Catalog.GameVersion.HasValue)
			{
				var version = await db.GameVersions.FindAsync(Catalog.GameVersion.Value);
				if (version is null)
				{
					ModelState.AddModelError($"{nameof(Catalog)}.{nameof(Catalog.GameVersion)}", $"Unknown Game Version Id: {Catalog.GameVersion.Value}");
				}
				else
				{
					externalMessages.Add($"Game Version changed from \"{submission.GameVersion?.Name}\" to \"{version.Name}\"");
					submission.GameVersionId = Catalog.GameVersion.Value;
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

		submission.EmulatorVersion = Catalog.Emulator;
		submission.AdditionalSyncNotes = Catalog.AdditionalSyncNotes;
		submission.GenerateTitle();

		bool synced = false;
		bool unsynced = false;
		if (Catalog.CanSyncVerify)
		{
			if (Catalog.SyncVerified)
			{
				if (!submission.SyncedOn.HasValue)
				{
					submission.SyncedOn = DateTime.UtcNow;
					submission.SyncedByUserId = User.GetUserId();
					synced = true;
				}
			}
			else
			{
				submission.SyncedOn = null;
				submission.SyncedByUserId = null;
				unsynced = true;
			}
		}

		var result = await db.TrySaveChanges();
		SetMessage(result, $"{Id}S catalog updated", $"Unable to save {Id}S catalog");
		if (result.IsSuccess())
		{
			string message = $"[{Id}S]({{0}}) Catalog edited by {User.Name()}";
			if (synced)
			{
				message += " (Sync Verified)";
			}
			else if (unsynced)
			{
				message += " (Sync Verified REMOVED)";
			}

			await publisher.SendGameManagement(
				message,
				$"{string.Join(", ", externalMessages)} | {submission.Title}",
				$"{Id}S");
		}

		return RedirectToPage("View", new { Id });
	}

	private async Task PopulateCatalogDropDowns()
	{
		AvailableGames = await db.Games.ToDropDownList(Catalog.System);
		AvailableVersions = await db.GameVersions.ToDropDownList(Catalog.System, Catalog.Game);
		AvailableSystems = await db.GameSystems.ToDropDownListWithId();
		AvailableSystemFrameRates = Catalog.System.HasValue
			? await db.GameSystemFrameRates.ToDropDownList(Catalog.System.Value)
			: [];

		AvailableGoals = Catalog.Game.HasValue
			? await db.GameGoals.ToDropDownList(Catalog.Game.Value)
			: [];
	}

	public class SubmissionCatalog
	{
		public string Title { get; init; } = "";
		public int? GameVersion { get; set; }
		public int? Game { get; set; }

		[Required]
		public int? System { get; set; }

		[Required]
		public int? SystemFramerate { get; init; }

		public int? Goal { get; set; }

		[StringLength(50)]
		public string? Emulator { get; init; }

		public bool SyncVerified { get; init; }
		public DateTime? SyncVerifiedOn { get; init; }
		public string? SyncedBy { get; init; }

		[StringLength(3000)]
		public string? AdditionalSyncNotes { get; init; }

		public bool CanSyncVerify => !string.IsNullOrWhiteSpace(Emulator) && this is
		{
			Game: not null,
			GameVersion: not null,
			SystemFramerate: not null,
			System: not null
		};
	}
}
