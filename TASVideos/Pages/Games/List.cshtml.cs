using TASVideos.Data.Entity.Game;

namespace TASVideos.Pages.Games;

public class ListModel(ApplicationDbContext db) : BasePageModel
{
	[FromQuery]
	[StringLength(50, MinimumLength = 3)]
	[Display(Name = "Search")]
	public string? SearchTerms { get; set; }

	[FromQuery]
	public GameListRequest Search { get; set; } = new();

	public SystemPageOf<GameEntry> Games { get; set; } = new([]);

	public List<SelectListItem> SystemList { get; set; } = [];

	public List<SelectListItem> LetterList { get; set; } = [];

	public List<SelectListItem> GenreList { get; set; } = [];

	public List<SelectListItem> GroupList { get; set; } = [];

	public async Task OnGet()
	{
		if (ModelState.IsValid)
		{
			Search.SearchTerms = SearchTerms;
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

	private async Task<SystemPageOf<GameEntry>> GetPageOfGames(GameListRequest paging)
	{
		IQueryable<Game> query = db.Games
			.ForSystemCode(paging.System)
			.ForGenre(paging.Genre)
			.ForGroup(paging.Group)
			.Where(g => g.DisplayName.StartsWith(paging.Letter ?? ""));

		if (!string.IsNullOrWhiteSpace(paging.SearchTerms))
		{
			db.ExtendTimeoutForSearch();
			query = query.WebSearch(paging.SearchTerms);
		}

		var data = await query.Select(g => new GameEntry
		{
			Id = g.Id,
			Name = g.DisplayName,
			Systems = g.GameVersions.Select(v => v.System!.Code).ToList()
		})
		.SortedPageOf(paging);

		return new SystemPageOf<GameEntry>(data)
		{
			System = paging.System,
			Letter = paging.Letter,
			Genre = paging.Genre,
			Group = paging.Group,
			SearchTerms = paging.SearchTerms,
		};
	}

	public class GameListRequest : PagingModel
	{
		public GameListRequest()
		{
			PageSize = 50;
			Sort = "Name";
		}

		public string? System { get; set; }

		public string? Letter { get; init; }

		public string? Genre { get; init; }

		public string? Group { get; init; }

		public string? SearchTerms { get; set; }
	}

	public class SystemPageOf<T>(IEnumerable<T> items) : PageOf<T>(items)
	{
		public string? System { get; init; }

		[Display(Name = "Starts with")]
		public string? Letter { get; init; }
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
