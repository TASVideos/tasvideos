using System.ComponentModel.DataAnnotations;
using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using TASVideos.Core.Services;
using TASVideos.Data;
using TASVideos.Data.Entity;
using TASVideos.Data.Entity.Game;
using TASVideos.Pages.Games.Models;

namespace TASVideos.Pages.Games;

[RequirePermission(PermissionTo.CatalogMovies)]
public class EditModel : BasePageModel
{
	private readonly ApplicationDbContext _db;
	private readonly IWikiPages _wikiPages;
	private readonly IMapper _mapper;

	public EditModel(
		ApplicationDbContext db,
		IWikiPages wikiPages,
		IMapper mapper)
	{
		_db = db;
		_wikiPages = wikiPages;
		_mapper = mapper;
	}

	[FromRoute]
	public int? Id { get; set; }

	[FromQuery]
	public int? SystemId { get; set; }

	[BindProperty]
	public GameEditModel Game { get; set; } = new();

	public bool CanDelete { get; set; }
	public IEnumerable<SelectListItem> AvailableSystems { get; set; } = new List<SelectListItem>();

	[Display(Name = "Available Genres")]
	public IEnumerable<SelectListItem> AvailableGenres { get; set; } = new List<SelectListItem>();

	[Display(Name = "Available Groups")]
	public IEnumerable<SelectListItem> AvailableGroups { get; set; } = new List<SelectListItem>();

	public async Task<IActionResult> OnGet()
	{
		if (Id.HasValue)
		{
			var game = await _mapper.ProjectTo<GameEditModel>(
				_db.Games.Where(g => g.Id == Id))
				.SingleOrDefaultAsync();

			if (game is null)
			{
				return NotFound();
			}

			Game = game;
		}
		else if (SystemId.HasValue)
		{
			var systemCode = await _db.GameSystems
				.Where(s => s.Id == SystemId)
				.Select(s => s.Code)
				.SingleOrDefaultAsync();
			if (systemCode is not null)
			{
				Game.SystemCode = systemCode;
			}
		}

		await Initialize();
		return Page();
	}

	public async Task<IActionResult> OnPost()
	{
		if (!ModelState.IsValid)
		{
			await Initialize();
			return Page();
		}

		if (!string.IsNullOrEmpty(Game.GameResourcesPage))
		{
			var page = await _wikiPages.Page(Game.GameResourcesPage);
			if (page is null)
			{
				ModelState.AddModelError($"{nameof(Game)}.{nameof(Game.GameResourcesPage)}", $"Page {Game.GameResourcesPage} not found");
				await Initialize();
				return Page();
			}
		}

		var system = await _db.GameSystems
			.SingleOrDefaultAsync(s => s.Code == Game.SystemCode);

		if (system is null)
		{
			return BadRequest();
		}

		Game? game;
		if (Id.HasValue)
		{
			game = await _db.Games
				.Include(g => g.GameGenres)
				.Include(g => g.GameGroups)
				.SingleOrDefaultAsync(g => g.Id == Id.Value);
			if (game is null)
			{
				return NotFound();
			}

			_mapper.Map(Game, game);
			SetGameValues(game, Game, system);
			await ConcurrentSave(_db, $"Game {Id} updated", $"Unable to update Game {Id}");
		}
		else
		{
			game = _mapper.Map<Game>(Game);
			_db.Games.Add(game);
			SetGameValues(game, Game, system);
			await ConcurrentSave(_db, $"Game {game.GoodName} created", "Unable to create game");
		}

		return BasePageRedirect("Index", new { game.Id });
	}

	private static void SetGameValues(Game game, GameEditModel editModel, GameSystem system)
	{
		game.GameResourcesPage = editModel.GameResourcesPage;
		game.System = system;
		game.GameGenres.SetGenres(editModel.Genres);
		game.GameGroups.SetGroups(editModel.Groups);
	}

	public async Task<IActionResult> OnPostDelete()
	{
		if (!Id.HasValue)
		{
			return NotFound();
		}

		if (!await CanBeDeleted())
		{
			ErrorStatusMessage($"Unable to delete Game {Id}, game is used by a publication or submission.");
			return BasePageRedirect("List");
		}

		_db.Games.Attach(new Game { Id = Id ?? 0 }).State = EntityState.Deleted;
		await ConcurrentSave(_db, $"Game {Id} deleted", $"Unable to delete Game {Id}");

		return BasePageRedirect("List");
	}

	private async Task Initialize()
	{
		AvailableSystems = await _db.GameSystems
			.OrderBy(s => s.Code)
			.ToDropdown()
			.ToListAsync();

		AvailableGenres = await _db.Genres
			.OrderBy(g => g.DisplayName)
			.ToDropdown()
			.ToListAsync();

		AvailableGroups = await _db.GameGroups
			.OrderBy(g => g.Name)
			.ToDropdown()
			.ToListAsync();

		CanDelete = await CanBeDeleted();
	}

	private async Task<bool> CanBeDeleted()
	{
		return Id > 0
			&& !await _db.Submissions.AnyAsync(s => s.Game!.Id == Id)
			&& !await _db.Publications.AnyAsync(p => p.Game!.Id == Id);
	}
}
