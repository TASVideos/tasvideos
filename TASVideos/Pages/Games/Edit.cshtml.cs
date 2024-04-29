using Microsoft.AspNetCore.Mvc.Rendering;
using TASVideos.Core.Services.ExternalMediaPublisher;
using TASVideos.Core.Services.Wiki;
using TASVideos.Core.Settings;
using TASVideos.Data.Entity.Game;

namespace TASVideos.Pages.Games;

[RequirePermission(PermissionTo.CatalogMovies)]
public class EditModel(
	ApplicationDbContext db,
	IWikiPages wikiPages,
	ExternalMediaPublisher publisher,
	AppSettings settings)
	: BasePageModel
{
	private readonly string _baseUrl = settings.BaseUrl;

	[FromRoute]
	public int? Id { get; set; }

	[BindProperty]
	public GameEdit Game { get; set; } = new();

	public bool CanDelete { get; set; }

	public List<SelectListItem> AvailableGenres { get; set; } = [];
	public List<SelectListItem> AvailableGroups { get; set; } = [];

	public async Task<IActionResult> OnGet()
	{
		if (Id.HasValue)
		{
			var game = await db.Games
				.Where(g => g.Id == Id)
				.Select(g => new GameEdit
				{
					DisplayName = g.DisplayName,
					Abbreviation = g.Abbreviation,
					Aliases = g.Aliases,
					ScreenshotUrl = g.ScreenshotUrl,
					GameResourcesPage = g.GameResourcesPage,
					Genres = g.GameGenres.Select(gg => gg.GenreId).ToList(),
					Groups = g.GameGroups.Select(gg => gg.GameGroupId).ToList()
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
		Game.DisplayName = Game.DisplayName.Trim();
		Game.Abbreviation = Game.Abbreviation?.Trim();
		Game.GameResourcesPage = Game.GameResourcesPage
			?.Replace(_baseUrl, "")
			.Trim()
			.Trim('/');
		Game.Aliases = Game.Aliases?.Trim().Replace(", ", ",");

		if (!ModelState.IsValid)
		{
			await Initialize();
			return Page();
		}

		if (!string.IsNullOrEmpty(Game.GameResourcesPage))
		{
			var page = await wikiPages.Page(Game.GameResourcesPage);
			if (page is null)
			{
				ModelState.AddModelError($"{nameof(Game)}.{nameof(Game.GameResourcesPage)}", $"Page {Game.GameResourcesPage} not found");
			}
		}

		if (Game.Abbreviation is not null && await db.Games.AnyAsync(g => g.Id != Id && g.Abbreviation == Game.Abbreviation))
		{
			ModelState.AddModelError($"{nameof(Game)}.{nameof(Game.Abbreviation)}", $"Abbreviation {Game.Abbreviation} already exists");
		}

		if (!ModelState.IsValid)
		{
			await Initialize();
			return Page();
		}

		Game? game;
		var action = "created";
		if (Id.HasValue)
		{
			action = "updated";
			game = await db.Games
				.Include(g => g.GameGenres)
				.Include(g => g.GameGroups)
				.SingleOrDefaultAsync(g => g.Id == Id.Value);
			if (game is null)
			{
				return NotFound();
			}
		}
		else
		{
			game = new Game();
			db.Games.Add(game);
			db.GameGoals.Add(new GameGoal
			{
				Game = game,
				DisplayName = "baseline"
			});
		}

		game.DisplayName = Game.DisplayName;
		game.Abbreviation = Game.Abbreviation;
		game.Aliases = Game.Aliases;
		game.ScreenshotUrl = Game.ScreenshotUrl;
		game.GameResourcesPage = Game.GameResourcesPage;

		SetGameValues(game, Game);
		var saveResult = await ConcurrentSave(db, $"Game {game.DisplayName} {action}", $"Unable to update Game {game.DisplayName}");
		if (saveResult && !Game.MinorEdit)
		{
			await publisher.SendGameManagement(
				$"Game [{game.DisplayName}]({{0}}) {action} by {User.Name()}",
				"",
				$"{Id}G");
		}

		return BasePageRedirect("Index", new { game.Id });
	}

	private static void SetGameValues(Game game, GameEdit edit)
	{
		game.GameResourcesPage = edit.GameResourcesPage;
		game.GameGenres.SetGenres(edit.Genres);
		game.GameGroups.SetGroups(edit.Groups);
	}

	public async Task<IActionResult> OnPostDelete()
	{
		if (!Id.HasValue)
		{
			return NotFound();
		}

		if (!User.Has(PermissionTo.DeleteGameEntries))
		{
			return AccessDenied();
		}

		if (!await CanBeDeleted())
		{
			ErrorStatusMessage($"Unable to delete Game {Id}, game is used by a publication or submission.");
			return BasePageRedirect("List");
		}

		var game = await db.Games.SingleOrDefaultAsync(g => g.Id == Id);
		if (game is null)
		{
			return NotFound();
		}

		db.Games.Remove(game);
		var saveMessage = $"Game #{Id} {game.DisplayName} deleted";
		var saveResult = await ConcurrentSave(db, saveMessage, $"Unable to delete Game {Id}");
		if (saveResult)
		{
			await publisher.SendMessage(PostGroups.Game, $"{saveMessage} by {User.Name()}");
		}

		return BasePageRedirect("List");
	}

	private async Task Initialize()
	{
		AvailableGenres = await db.Genres
			.OrderBy(g => g.DisplayName)
			.ToDropDown()
			.ToListAsync();

		AvailableGroups = await db.GameGroups
			.OrderBy(g => g.Name)
			.ToDropDown()
			.ToListAsync();

		CanDelete = await CanBeDeleted();
	}

	private async Task<bool> CanBeDeleted()
	{
		return Id > 0
			&& !await db.Submissions.AnyAsync(s => s.GameId == Id)
			&& !await db.Publications.AnyAsync(p => p.GameId == Id)
			&& !await db.UserFiles.AnyAsync(u => u.GameId == Id);
	}

	public class GameEdit
	{
		[StringLength(100)]
		[Display(Name = "Display Name")]
		public string DisplayName { get; set; } = "";

		[StringLength(24)]
		public string? Abbreviation { get; set; }

		[StringLength(250)]
		public string? Aliases { get; set; }

		[StringLength(250)]
		[Display(Name = "Screenshot URL")]
		public string? ScreenshotUrl { get; init; }

		[StringLength(300)]
		[Display(Name = "Game Resources Page")]
		public string? GameResourcesPage { get; set; }
		public List<int> Genres { get; init; } = [];
		public List<int> Groups { get; init; } = [];
		public bool MinorEdit { get; init; }
	}
}
