using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using TASVideos.Data;
using TASVideos.Data.Entity;
using TASVideos.Data.Entity.Game;
using TASVideos.Pages.Games.Versions.Models;

namespace TASVideos.Pages.Games.Versions;

[RequirePermission(PermissionTo.CatalogMovies)]
public class EditModel(ApplicationDbContext db) : BasePageModel
{
	private static readonly IEnumerable<SelectListItem> VersionTypes = Enum
		.GetValues(typeof(VersionTypes))
		.Cast<VersionTypes>()
		.Select(r => new SelectListItem
		{
			Text = r.ToString(),
			Value = ((int)r).ToString()
		});

	[FromRoute]
	public int GameId { get; set; }

	[FromRoute]
	public int? Id { get; set; }

	[FromQuery]
	public int? SystemId { get; set; }

	[BindProperty]
	public VersionEditModel Version { get; set; } = new();

	[BindProperty]
	public string GameName { get; set; } = "";

	public bool CanDelete { get; set; }
	public IEnumerable<SelectListItem> AvailableSystems { get; set; } = [];
	public IEnumerable<SelectListItem> AvailableVersionTypes => VersionTypes;

	public IEnumerable<SelectListItem> AvailableRegionTypes { get; set; } = new SelectListItem[]
	{
			new () { Text = "U", Value = "U" },
			new () { Text = "J", Value = "J" },
			new () { Text = "E", Value = "E" },
			new () { Text = "JU", Value = "JU" },
			new () { Text = "EU", Value = "UE" },
			new () { Text = "W", Value = "W" },
			new () { Text = "Other", Value = "Other" },
	};

	public async Task<IActionResult> OnGet()
	{
		var game = await db.Games.SingleOrDefaultAsync(g => g.Id == GameId);

		if (game is null)
		{
			return NotFound();
		}

		GameName = game.DisplayName;

		AvailableSystems = await db.GameSystems
			.OrderBy(s => s.Code)
			.ToDropdown()
			.ToListAsync();

		if (SystemId.HasValue)
		{
			var systemCode = await db.GameSystems
				.Where(s => s.Id == SystemId)
				.Select(s => s.Code)
				.SingleOrDefaultAsync();
			if (systemCode is not null)
			{
				Version.SystemCode = systemCode;
			}
		}

		if (!Id.HasValue)
		{
			return Page();
		}

		var version = await db.GameVersions
			.Where(r => r.Id == Id.Value && r.Game!.Id == GameId)
			.Select(v => new VersionEditModel
			{
				SystemCode = v.System!.Code,
				Name = v.Name,
				Md5 = v.Md5,
				Sha1 = v.Sha1,
				Version = v.Version,
				Region = v.Region,
				Type = v.Type,
				TitleOverride = v.TitleOverride
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
			return Page();
		}

		var system = await db.GameSystems
			.SingleOrDefaultAsync(s => s.Code == Version.SystemCode);

		if (system is null)
		{
			return BadRequest();
		}

		GameVersion version;
		if (Id.HasValue)
		{
			version = await db.GameVersions.SingleAsync(r => r.Id == Id.Value);
			version.Name = Version.Name;
			version.Md5 = Version.Md5;
			version.Sha1 = Version.Sha1;
			version.Version = Version.Version;
			version.Region = Version.Region;
			version.Type = Version.Type;
			version.TitleOverride = Version.TitleOverride;
			version.System = system;
		}
		else
		{
			version = new GameVersion
			{
				Name = Version.Name,
				Md5 = Version.Md5,
				Sha1 = Version.Sha1,
				Version = Version.Version,
				Region = Version.Region,
				Type = Version.Type,
				TitleOverride = Version.TitleOverride,
				Game = await db.Games.SingleAsync(g => g.Id == GameId),
				System = system
			};
			db.GameVersions.Add(version);
		}

		try
		{
			await ConcurrentSave(db, $"Game Version {Id} updated", $"Unable to update Game Version {Id}");
		}
		catch (DbUpdateException ex)
		{
			ModelState.AddModelError("", ex.InnerException?.Message ?? ex.Message);
			return Page();
		}

		return string.IsNullOrWhiteSpace(HttpContext.Request.ReturnUrl())
			? RedirectToPage("List", new { gameId = GameId })
			: BaseReturnUrlRedirect($"?GameId={GameId}&GameVersionId={version.Id}");
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
		await ConcurrentSave(db, $"Game Version {Id} deleted", $"Unable to delete Game Version {Id}");
		return BasePageRedirect("List", new { gameId = GameId });
	}

	private async Task<bool> CanBeDeleted()
	{
		return !await db.Submissions.AnyAsync(s => s.GameVersion!.Id == Id)
				&& !await db.Publications.AnyAsync(p => p.GameVersion!.Id == Id);
	}
}
