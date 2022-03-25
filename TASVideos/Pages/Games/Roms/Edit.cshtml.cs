using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using TASVideos.Data;
using TASVideos.Data.Entity;
using TASVideos.Data.Entity.Game;
using TASVideos.Pages.Games.Roms.Models;

namespace TASVideos.Pages.Games.Roms;

[RequirePermission(PermissionTo.CatalogMovies)]
public class EditModel : BasePageModel
{
	private static readonly IEnumerable<SelectListItem> RomTypes = Enum
		.GetValues(typeof(RomTypes))
		.Cast<RomTypes>()
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

	[BindProperty]
	public RomEditModel Rom { get; set; } = new();

	[BindProperty]
	public string SystemCode { get; set; } = "";

	[BindProperty]
	public string GameName { get; set; } = "";

	public bool CanDelete { get; set; }
	public IEnumerable<SelectListItem> AvailableRomTypes => RomTypes;

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
		var game = await _db.Games
			.Include(g => g.System)
			.SingleOrDefaultAsync(g => g.Id == GameId);

		if (game is null)
		{
			return NotFound();
		}

		GameName = game.DisplayName;
		SystemCode = game.System!.Code;

		if (!Id.HasValue)
		{
			return Page();
		}

		Rom = await _mapper.ProjectTo<RomEditModel>(
			_db.GameRoms.Where(r => r.Id == Id.Value && r.Game!.Id == GameId))
			.SingleAsync();

		if (Rom is null)
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

		GameRom rom;
		if (Id.HasValue)
		{
			rom = await _db.GameRoms.SingleAsync(r => r.Id == Id.Value);
			_mapper.Map(Rom, rom);
		}
		else
		{
			rom = _mapper.Map<GameRom>(Rom);
			rom.Game = await _db.Games.SingleAsync(g => g.Id == GameId);
			_db.GameRoms.Add(rom);
		}

		try
		{
			await ConcurrentSave(_db, $"Rom {Id} updated", $"Unable to update Rom {Id}");
		}
		catch (DbUpdateException ex)
		{
			ModelState.AddModelError("", ex.InnerException?.Message ?? ex.Message);
			return Page();
		}

		return string.IsNullOrWhiteSpace(HttpContext.Request.ReturnUrl())
			? RedirectToPage("List", new { gameId = GameId })
			: BaseReturnUrlRedirect($"?GameId={GameId}&RomId={rom.Id}");
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
			return BasePageRedirect("List");
		}

		_db.GameRoms.Attach(new GameRom { Id = Id ?? 0 }).State = EntityState.Deleted;
		await ConcurrentSave(_db, $"Rom {Id} deleted", $"Unable to delete Rom {Id}");
		return BasePageRedirect("List");
	}

	private async Task<bool> CanBeDeleted()
	{
		return !await _db.Submissions.AnyAsync(s => s.Rom!.Id == Id)
				&& !await _db.Publications.AnyAsync(p => p.Rom!.Id == Id);
	}
}
