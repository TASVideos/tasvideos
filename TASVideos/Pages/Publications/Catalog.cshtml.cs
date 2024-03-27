using System.Globalization;
using Microsoft.AspNetCore.Mvc.Rendering;
using TASVideos.Core.Services.ExternalMediaPublisher;
using TASVideos.Data.Entity.Game;

namespace TASVideos.Pages.Publications;

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
	public PublicationCatalogModel Catalog { get; set; } = new();

	public List<SelectListItem> AvailableVersions { get; set; } = [];
	public List<SelectListItem> AvailableGames { get; set; } = [];
	public List<SelectListItem> AvailableSystems { get; set; } = [];
	public List<SelectListItem> AvailableSystemFrameRates { get; set; } = [];
	public List<SelectListItem> AvailableGoals { get; set; } = [];

	public async Task<IActionResult> OnGet()
	{
		var catalog = await db.Publications
				.Where(p => p.Id == Id)
				.Select(p => new PublicationCatalogModel
				{
					Title = p.Title,
					GameVersionId = p.GameVersionId,
					GameId = p.GameId,
					SystemId = p.SystemId,
					SystemFrameRateId = p.SystemFrameRateId,
					GameGoalId = p.GameGoalId!.Value
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
					var gameVersion = await db.GameVersions.SingleOrDefaultAsync(r => r.GameId == game.Id && r.Id == GameVersionId && r.SystemId == Catalog.SystemId);
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

		var publication = await db.Publications
			.IncludeTitleTables()
			.SingleOrDefaultAsync(s => s.Id == Id);
		if (publication is null)
		{
			return NotFound();
		}

		var externalMessages = new List<string>();

		if (publication.SystemId != Catalog.SystemId)
		{
			var system = await db.GameSystems.SingleOrDefaultAsync(s => s.Id == Catalog.SystemId);
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
			var systemFramerate = await db.GameSystemFrameRates.SingleOrDefaultAsync(s => s.Id == Catalog.SystemFrameRateId);
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
			var game = await db.Games.SingleOrDefaultAsync(s => s.Id == Catalog.GameId);
			if (game is null)
			{
				ModelState.AddModelError($"{nameof(Catalog)}.{nameof(Catalog.GameId)}", $"Unknown System Id: {Catalog.GameId}");
			}
			else
			{
				externalMessages.Add($"Game changed from \"{publication.Game!.DisplayName}\" to \"{game.DisplayName}\"");
				publication.GameId = Catalog.GameId;
				publication.Game = game;
			}
		}

		if (publication.GameGoalId != Catalog.GameGoalId)
		{
			var gameGoal = await db.GameGoals.SingleOrDefaultAsync(gg => gg.Id == Catalog.GameGoalId);
			if (gameGoal is null)
			{
				ModelState.AddModelError($"{nameof(Catalog)}.{nameof(Catalog.GameGoalId)}", $"Unknown Game Goal Id: {Catalog.GameGoalId}");
			}
			else
			{
				externalMessages.Add($"Game Goal changed from \"{publication.GameGoal!.DisplayName}\" to \"{gameGoal.DisplayName}\"");
				publication.GameGoalId = Catalog.GameGoalId;
				publication.GameGoal = gameGoal;
			}
		}

		if (publication.GameVersionId != Catalog.GameVersionId)
		{
			var gameVersion = await db.GameVersions.SingleOrDefaultAsync(s => s.Id == Catalog.GameVersionId);
			if (gameVersion is null)
			{
				ModelState.AddModelError($"{nameof(Catalog)}.{nameof(Catalog.GameVersionId)}", $"Unknown System Id: {Catalog.GameVersionId}");
			}
			else
			{
				externalMessages.Add($"Game Version changed from \"{publication.GameVersion!.Name}\" to \"{gameVersion.Name}\"");
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

		var result = await ConcurrentSave(db, $"{Id}M catalog updated", $"Unable to save {Id}M catalog");
		if (result && !Catalog.MinorEdit)
		{
			await publisher.SendGameManagement(
				$"{Id}M Catalog edited by {User.Name()}",
				$"[{Id}M]({{0}}) Catalog edited by {User.Name()}",
				$"{string.Join(", ", externalMessages)} | {publication.Title}",
				$"{Id}M");
		}

		return RedirectToPage("View", new { Id });
	}

	private async Task PopulateCatalogDropDowns(int gameId, int systemId)
	{
		AvailableVersions =
		[
			.. UiDefaults.DefaultEntry,
			.. await db.GameVersions
				.ForGame(gameId)
				.ForSystem(systemId)
				.OrderBy(r => r.Name)
				.ToDropDown()
				.ToListAsync(),
		];

		AvailableGames = await db.Games
			.ForSystem(systemId)
			.OrderBy(g => g.DisplayName)
			.ToDropDown()
			.ToListAsync();

		AvailableSystems = await db.GameSystems
			.OrderBy(s => s.Code)
			.ToDropDownWithId()
			.ToListAsync();

		AvailableSystemFrameRates = await db.GameSystemFrameRates
			.ForSystem(systemId)
			.ToDropDown()
			.ToListAsync();

		AvailableGoals = await db.GameGoals
			.Where(gg => gg.GameId == gameId)
			.ToDropDown()
			.ToListAsync();
	}

	public class PublicationCatalogModel
	{
		public string Title { get; set; } = "";

		[Display(Name = "Game Version")]
		public int GameVersionId { get; set; }

		[Display(Name = "Goal")]
		public int GameGoalId { get; set; }

		[Display(Name = "Game")]
		public int GameId { get; set; }

		[Display(Name = "System")]
		public int SystemId { get; set; }

		[Display(Name = "System Framerate")]
		public int SystemFrameRateId { get; set; }

		public bool MinorEdit { get; set; }
	}
}
