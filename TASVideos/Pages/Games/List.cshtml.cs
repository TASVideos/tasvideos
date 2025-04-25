using TASVideos.Data.Entity.Game;

namespace TASVideos.Pages.Games;

public class ListModel(ApplicationDbContext db) : BasePageModel
{
	[FromQuery]
	[StringLength(50, MinimumLength = 3)]
	public string? SearchTerms { get; set; }

	[FromQuery]
	public GameListRequest Search { get; set; } = new();

	public PageOf<GameEntry, GameListRequest> Games { get; set; } = new([], new());

	public List<SelectListItem> SystemList { get; set; } = [];

	public List<SelectListItem> LetterList { get; set; } = [];

	public List<SelectListItem> GenreList { get; set; } = [];

	public List<SelectListItem> GroupList { get; set; } = [];

	public async Task OnGet()
	{
		if (ModelState.IsValid)
		{
			Games = await GetPageOfGames(Search);
		}

		SystemList = (await db.GameSystems
			.ToDropDownList())
			.WithAnyEntry();

		LetterList = (await db.Games
			.Select(g => g.DisplayName.Substring(0, 1))
			.Distinct()
			.ToDropDownList())
			.WithAnyEntry();

		GenreList = (await db.Genres
			.Select(g => g.DisplayName)
			.Distinct()
			.ToDropDownList())
			.WithAnyEntry();

		GroupList = (await db.GameGroups
			.Select(g => g.Name)
			.Distinct()
			.ToDropDownList())
			.WithAnyEntry();
	}

	public async Task<IActionResult> OnGetFrameRateDropDownForSystem(int systemId, bool includeEmpty)
	{
		var items = await db.GameSystemFrameRates.ToDropDownList(systemId);
		return ToDropdownResult(items, includeEmpty);
	}

	public async Task<IActionResult> OnGetGameDropDownForSystem(int systemId, bool includeEmpty)
	{
		var items = await db.Games.ToDropDownList(systemId);
		return ToDropdownResult(items, includeEmpty);
	}

	public async Task<IActionResult> OnGetVersionDropDownForGame(int gameId, int systemId, bool includeEmpty)
	{
		var items = await db.GameVersions.ToDropDownList(systemId, gameId);
		return ToDropdownResult(items, includeEmpty);
	}

	public async Task<IActionResult> OnGetGameGoalDropDownForGame(int gameId, bool includeEmpty)
	{
		var items = await db.GameGoals.ToDropDownList(gameId);
		return ToDropdownResult(items, includeEmpty);
	}

	private async Task<PageOf<GameEntry, GameListRequest>> GetPageOfGames(GameListRequest paging)
	{
		IQueryable<Game> query = db.Games
			.ForSystemCode(paging.System)
			.ForGenre(paging.Genre)
			.ForGroup(paging.Group)
			.Where(g => g.DisplayName.StartsWith(paging.StartsWith ?? ""));

		if (!string.IsNullOrWhiteSpace(paging.SearchTerms))
		{
			db.ExtendTimeoutForSearch();
			query = query.WebSearch(paging.SearchTerms);
		}

		return await query.Select(g => new GameEntry
		{
			Id = g.Id,
			Name = g.DisplayName,
			Systems = g.GameVersions.Select(v => v.System!.Code).ToList()
		})
		.SortedPageOf(paging);
	}

	[PagingDefaults(PageSize = 50, Sort = "Name")]
	public class GameListRequest : PagingModel
	{
		public string? System { get; set; }

		public string? StartsWith { get; init; }

		public string? Genre { get; init; }

		public string? Group { get; init; }

		public string? SearchTerms { get; init; }
	}

	public class GameEntry
	{
		[Sortable]
		public int Id { get; init; }

		[Sortable]
		public string Name { get; init; } = "";
		public List<string> Systems { get; init; } = [];
	}
}
