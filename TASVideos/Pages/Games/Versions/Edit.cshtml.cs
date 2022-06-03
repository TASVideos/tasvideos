using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using TASVideos.Data;
using TASVideos.Data.Entity;
using TASVideos.Data.Entity.Game;
using TASVideos.Pages.Games.Versions.Models;

namespace TASVideos.Pages.Games.Versions;

[RequirePermission(PermissionTo.CatalogMovies)]
public class EditModel : BasePageModel
{
	private static readonly IEnumerable<SelectListItem> VersionTypes = Enum
		.GetValues(typeof(VersionTypes))
		.Cast<VersionTypes>()
		.Select(r => new SelectListItem
		{
			Text = r.ToString(),
			Value = ((int)r).ToString()
		});

	private readonly ApplicationDbContext _db;
	private readonly IMapper _mapper;

	public EditModel(
		ApplicationDbContext db,
		IMapper mapper)
	{
		_db = db;
		_mapper = mapper;
	}

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
	public IEnumerable<SelectListItem> AvailableSystems { get; set; } = new List<SelectListItem>();
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
		var game = await _db.Games.SingleOrDefaultAsync(g => g.Id == GameId);

		if (game is null)
		{
			return NotFound();
		}

		GameName = game.DisplayName;

		AvailableSystems = await _db.GameSystems
			.OrderBy(s => s.Code)
			.ToDropdown()
			.ToListAsync();

		if (SystemId.HasValue)
		{
			var systemCode = await _db.GameSystems
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

		Version = await _mapper.ProjectTo<VersionEditModel>(
			_db.GameVersions.Where(r => r.Id == Id.Value && r.Game!.Id == GameId))
			.SingleAsync();

		if (Version is null)
		{
			return NotFound();
		}

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

		var system = await _db.GameSystems
			.SingleOrDefaultAsync(s => s.Code == Version.SystemCode);

		if (system is null)
		{
			return BadRequest();
		}

		GameVersion version;
		if (Id.HasValue)
		{
			version = await _db.GameVersions.SingleAsync(r => r.Id == Id.Value);
			version.System = system;
			_mapper.Map(Version, version);
		}
		else
		{
			version = _mapper.Map<GameVersion>(Version);
			version.Game = await _db.Games.SingleAsync(g => g.Id == GameId);
			version.System = system;
			_db.GameVersions.Add(version);
		}

		try
		{
			await ConcurrentSave(_db, $"Game Version {Id} updated", $"Unable to update Game Version {Id}");
		}
		catch (DbUpdateException ex)
		{
			ModelState.AddModelError("", ex.InnerException?.Message ?? ex.Message);
			return Page();
		}

		return string.IsNullOrWhiteSpace(HttpContext.Request.ReturnUrl())
			? RedirectToPage("List", new { gameId = GameId })
			: BaseReturnUrlRedirect($"?GameId={GameId}&RomId={version.Id}");
	}

	public async Task<IActionResult> OnPostDelete()
	{
		if (!Id.HasValue)
		{
			return NotFound();
		}

		if (!await CanBeDeleted())
		{
			ErrorStatusMessage($"Unable to delete Rom {Id}, rom is used by a publication or submission.");
			return BasePageRedirect("List", new { gameId = GameId });
		}

		_db.GameVersions.Attach(new GameVersion { Id = Id ?? 0 }).State = EntityState.Deleted;
		await ConcurrentSave(_db, $"Game Version {Id} deleted", $"Unable to delete Game Version {Id}");
		return BasePageRedirect("List", new { gameId = GameId });
	}

	private async Task<bool> CanBeDeleted()
	{
		return !await _db.Submissions.AnyAsync(s => s.GameVersion!.Id == Id)
				&& !await _db.Publications.AnyAsync(p => p.GameVersion!.Id == Id);
	}
}
