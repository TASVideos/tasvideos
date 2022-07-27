using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using TASVideos.Core.Services;
using TASVideos.Core.Services.ExternalMediaPublisher;
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
	private readonly ExternalMediaPublisher _publisher;

	public EditModel(
		ApplicationDbContext db,
		IWikiPages wikiPages,
		ExternalMediaPublisher publisher)
	{
		_db = db;
		_wikiPages = wikiPages;
		_publisher = publisher;
	}

	[FromRoute]
	public int? Id { get; set; }

	[BindProperty]
	public GameEditModel Game { get; set; } = new();

	public bool CanDelete { get; set; }

	[Display(Name = "Available Genres")]
	public IEnumerable<SelectListItem> AvailableGenres { get; set; } = new List<SelectListItem>();

	[Display(Name = "Available Groups")]
	public IEnumerable<SelectListItem> AvailableGroups { get; set; } = new List<SelectListItem>();

	public async Task<IActionResult> OnGet()
	{
		if (Id.HasValue)
		{
			var game = await _db.Games
				.Where(g => g.Id == Id)
				.Select(g => new GameEditModel
				{
					DisplayName = g.DisplayName,
					Abbreviation = g.Abbreviation,
					Aliases = g.Aliases,
					ScreenshotUrl = g.ScreenshotUrl,
					GameResourcesPage = g.GameResourcesPage,
					Genres = g.GameGenres.Select(gg => gg.GenreId),
					Groups = g.GameGroups.Select(gg => gg.GameGroupId)
				})
				.SingleOrDefaultAsync();

			if (game is null)
			{
				return NotFound();
			}

			Game = game;
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
			}
		}

		if (await _db.Games.AnyAsync(g => g.Id != Id && g.Abbreviation == Game.Abbreviation))
		{
			ModelState.AddModelError($"{nameof(Game)}.{nameof(Game.Abbreviation)}", $"Abbreviation {Game.Abbreviation} already exists");
		}

		if (!ModelState.IsValid)
		{
			await Initialize();
			return Page();
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

			game.DisplayName = Game.DisplayName;
			game.Abbreviation = Game.Abbreviation;
			game.Aliases = Game.Aliases;
			game.ScreenshotUrl = Game.ScreenshotUrl;
			game.GameResourcesPage = Game.GameResourcesPage;
			SetGameValues(game, Game);
			var saveMessage = $"Game {game.DisplayName} updated";
			var saveResult = await ConcurrentSave(_db, saveMessage, $"Unable to update Game {Id}");
			if (saveResult && !Game.MinorEdit)
			{
				await _publisher.SendGameManagement($"{saveMessage} by {User.Name()}", "", $"{Id}G");
			}
		}
		else
		{
			game = new Game
			{
				DisplayName = Game.DisplayName,
				Abbreviation = Game.Abbreviation,
				Aliases = Game.Aliases,
				ScreenshotUrl = Game.ScreenshotUrl,
				GameResourcesPage = Game.GameResourcesPage
			};
			_db.Games.Add(game);
			SetGameValues(game, Game);
			var saveMessage = $"Game {game.DisplayName} created";
			var saveResult = await ConcurrentSave(_db, saveMessage, "Unable to create game");
			if (saveResult && !Game.MinorEdit)
			{
				await _publisher.SendGameManagement($"{saveMessage} by {User.Name()}", "", $"{game.Id}G");
			}
		}

		return BasePageRedirect("Index", new { game.Id });
	}

	private static void SetGameValues(Game game, GameEditModel editModel)
	{
		game.GameResourcesPage = editModel.GameResourcesPage;
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

		var game = await _db.Games.SingleOrDefaultAsync(g => g.Id == Id);
		if (game is null)
		{
			return NotFound();
		}

		_db.Games.Remove(game);
		var saveMessage = $"Game #{Id} {game.DisplayName} deleted";
		var saveResult = await ConcurrentSave(_db, saveMessage, $"Unable to delete Game {Id}");
		if (saveResult)
		{
			await _publisher.SendGameManagement($"{saveMessage} by {User.Name()}", "", $"{Id}G");
		}

		return BasePageRedirect("List");
	}

	private async Task Initialize()
	{
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
