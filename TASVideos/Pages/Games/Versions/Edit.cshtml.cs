using TASVideos.Data.Entity.Game;

namespace TASVideos.Pages.Games.Versions;

[RequirePermission(PermissionTo.CatalogMovies)]
public class EditModel(ApplicationDbContext db, IExternalMediaPublisher publisher) : BasePageModel
{
	private static readonly List<SelectListItem> VersionTypes = Enum
		.GetValues<VersionTypes>()
		.ToDropDown();

	[FromRoute]
	public int GameId { get; set; }

	[FromRoute]
	public int? Id { get; set; }

	[FromQuery]
	public int? SystemId { get; set; }

	[BindProperty]
	public VersionEdit Version { get; set; } = new();

	[BindProperty]
	public string GameName { get; set; } = "";

	public bool CanDelete { get; set; }
	public List<SelectListItem> AvailableSystems { get; set; } = [];
	public List<SelectListItem> AvailableVersionTypes => VersionTypes;

	public List<SelectListItem> AvailableRegionTypes { get; set; } =
	[
		new() { Text = "U", Value = "U" },
		new() { Text = "J", Value = "J" },
		new() { Text = "E", Value = "E" },
		new() { Text = "JU", Value = "JU" },
		new() { Text = "EU", Value = "UE" },
		new() { Text = "W", Value = "W" },
		new() { Text = "Other", Value = "Other" }
	];

	public async Task<IActionResult> OnGet()
	{
		var game = await db.Games.SingleOrDefaultAsync(g => g.Id == GameId);

		if (game is null)
		{
			return NotFound();
		}

		GameName = game.DisplayName;

		AvailableSystems = (await db.GameSystems.ToDropDownList()).WithDefaultEntry();

		if (SystemId.HasValue)
		{
			var systemCode = await db.GameSystems
				.Where(s => s.Id == SystemId)
				.Select(s => s.Code)
				.SingleOrDefaultAsync();
			if (systemCode is not null)
			{
				Version.System = systemCode;
			}
		}

		if (!Id.HasValue)
		{
			return Page();
		}

		var version = await db.GameVersions
			.Where(r => r.Id == Id.Value && r.Game!.Id == GameId)
			.Select(v => new VersionEdit
			{
				System = v.System!.Code,
				Name = v.Name,
				Md5 = v.Md5,
				Sha1 = v.Sha1,
				Version = v.Version,
				Region = v.Region ?? "",
				Type = v.Type,
				TitleOverride = v.TitleOverride,
				SourceDb = v.SourceDb,
				Notes = v.Notes
			})
			.SingleOrDefaultAsync();

		if (version is null)
		{
			return NotFound();
		}

		Version = version;
		CanDelete = await CanBeDeleted();

		return Page();
	}

	public async Task<IActionResult> OnPost()
	{
		if (!ModelState.IsValid)
		{
			CanDelete = await CanBeDeleted();
			AvailableSystems = (await db.GameSystems.ToDropDownList()).WithDefaultEntry();
			return Page();
		}

		var system = await db.GameSystems.SingleOrDefaultAsync(s => s.Code == Version.System);
		if (system is null)
		{
			return BadRequest();
		}

		GameVersion version;
		var action = "created";
		if (Id.HasValue)
		{
			action = "updated";
			version = await db.GameVersions.SingleAsync(r => r.Id == Id.Value);
		}
		else
		{
			version = new GameVersion
			{
				Game = await db.Games.SingleAsync(g => g.Id == GameId)
			};
			db.GameVersions.Add(version);
		}

		version.Name = Version.Name;
		version.Md5 = Version.Md5;
		version.Sha1 = Version.Sha1;
		version.Version = Version.Version;
		version.Region = Version.Region;
		version.Type = Version.Type;
		version.TitleOverride = Version.TitleOverride;
		version.System = system;
		version.SourceDb = Version.SourceDb;
		version.Notes = Version.Notes;

		var saveResult = await db.TrySaveChanges();
		if (saveResult == SaveResult.UpdateFailure)
		{
			ModelState.AddModelError("", "Unable to save");
			return Page();
		}

		SetMessage(saveResult, $"Game Version {version.Name} {action}", $"Unable to update Game {version.Name}");
		if (saveResult.IsSuccess())
		{
			await publisher.SendGameManagement(
				$"Game Version [{version.Name}]({{0}}) {action} by {User.Name()}",
				"",
				$"/Games/{version.GameId}/Versions/View/{version.Id}");
		}

		return string.IsNullOrWhiteSpace(HttpContext.Request.ReturnUrl())
			? RedirectToPage("List", new { gameId = GameId })
			: BaseReturnUrlRedirect(new() { ["SystemId"] = system.Id.ToString(), ["GameId"] = GameId.ToString(), ["GameVersionId"] = version.Id.ToString() });
	}

	public async Task<IActionResult> OnPostDelete()
	{
		if (!Id.HasValue)
		{
			return NotFound();
		}

		if (!await CanBeDeleted())
		{
			ErrorStatusMessage($"Unable to delete Game Version {Id}, version is used by a publication or submission.");
			return BasePageRedirect("List", new { gameId = GameId });
		}

		db.GameVersions.Attach(new GameVersion { Id = Id ?? 0 }).State = EntityState.Deleted;
		var saveResult = await db.TrySaveChanges();
		string saveMessage = $"Game Version {Id} deleted";
		SetMessage(saveResult, saveMessage, $"Unable to delete Game Version {Id}");
		if (saveResult.IsSuccess())
		{
			await publisher.SendMessage(PostGroups.Game, $"{saveMessage} by {User.Name()}");
		}

		return BasePageRedirect("List", new { gameId = GameId });
	}

	private async Task<bool> CanBeDeleted()
	{
		return !await db.Submissions.AnyAsync(s => s.GameVersion!.Id == Id)
				&& !await db.Publications.AnyAsync(p => p.GameVersion!.Id == Id);
	}

	public class VersionEdit
	{
		[StringLength(8)]
		public string System { get; set; } = "";

		[StringLength(255)]
		public string Name { get; init; } = "";

		[RegularExpression("^[A-Fa-f0-9]*$")]
		[StringLength(32, MinimumLength = 32)]
		public string? Md5 { get; init; }

		[RegularExpression("^[A-Fa-f0-9]*$")]
		[StringLength(40, MinimumLength = 40)]
		public string? Sha1 { get; init; }

		[StringLength(50)]
		public string? Version { get; init; }

		[StringLength(50)]
		public string Region { get; init; } = "";
		public VersionTypes Type { get; init; }

		[StringLength(255)]
		public string? TitleOverride { get; init; }

		[StringLength(50)]
		public string? SourceDb { get; init; }

		[StringLength(30000)]
		public string? Notes { get; init; }
	}
}
