using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using TASVideos.Core;
using TASVideos.Data;
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

	public SystemPageOf<GameListModel> Games { get; set; } = SystemPageOf<GameListModel>.Empty();

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

		SystemList = await db.GameSystems
			.OrderBy(s => s.Code)
			.ToDropDown()
			.ToListAsync();

		SystemList.Insert(0, new SelectListItem { Text = "Any", Value = "" });

		LetterList = await db.Games
			.Select(g => g.DisplayName.Substring(0, 1))
			.Distinct()
			.OrderBy(l => l)
			.ToDropDown()
			.ToListAsync();

		LetterList.Insert(0, new SelectListItem { Text = "Any", Value = "" });

		GenreList = await db.Genres
			.Select(g => g.DisplayName)
			.Distinct()
			.OrderBy(l => l)
			.ToDropDown()
			.ToListAsync();

		GenreList.Insert(0, new SelectListItem { Text = "Any", Value = "" });

		GroupList = await db.GameGroups
			.Select(g => g.Name)
			.Distinct()
			.OrderBy(l => l)
			.ToDropDown()
			.ToListAsync();

		GroupList.Insert(0, new SelectListItem { Text = "Any", Value = "" });
	}

	public async Task<IActionResult> OnGetFrameRateDropDownForSystem(int systemId, bool includeEmpty)
	{
		var items = await db.GameSystemFrameRates
			.ForSystem(systemId)
			.ToDropDown()
			.ToListAsync();

		if (includeEmpty)
		{
			items = [.. UiDefaults.DefaultEntry, .. items];
		}

		return ToDropdownResult(items);
	}

	public async Task<IActionResult> OnGetGameDropDownForSystem(int systemId, bool includeEmpty)
	{
		var items = await db.Games
			.ForSystem(systemId)
			.OrderBy(g => g.DisplayName)
			.ToDropDown()
			.ToListAsync();

		if (includeEmpty)
		{
			items = [.. UiDefaults.DefaultEntry, .. items];
		}

		return ToDropdownResult(items);
	}

	public async Task<IActionResult> OnGetVersionDropDownForGame(int gameId, int systemId, bool includeEmpty)
	{
		var items = await db.GameVersions
			.ForGame(gameId)
			.ForSystem(systemId)
			.OrderBy(r => r.Name)
			.ToDropDown()
			.ToListAsync();

		if (includeEmpty)
		{
			items = [.. UiDefaults.DefaultEntry, .. items];
		}

		return ToDropdownResult(items);
	}

	public async Task<IActionResult> OnGetGameGoalDropDownForGame(int gameId, bool includeEmpty)
	{
		var items = await db.GameGoals
			.Where(gg => gg.GameId == gameId)
			.OrderBy(gg => gg.DisplayName)
			.ToDropDown()
			.ToListAsync();

		if (includeEmpty)
		{
			items = [.. UiDefaults.DefaultEntry, .. items];
		}

		return ToDropdownResult(items);
	}

	private async Task<SystemPageOf<GameListModel>> GetPageOfGames(GameListRequest paging)
	{
		PageOf<GameListModel> data;
		if (!string.IsNullOrWhiteSpace(paging.SearchTerms))
		{
			db.Database.SetCommandTimeout(TimeSpan.FromSeconds(30));
			data = await db.Games
				.ForSystemCode(paging.SystemCode)
				.ForGenre(paging.Genre)
				.ForGroup(paging.Group)
				.Where(g => g.DisplayName.StartsWith(paging.Letter ?? ""))
				.Where(g => EF.Functions.ToTsVector("simple", g.DisplayName + " || " + g.Aliases + " || " + g.Abbreviation).Matches(EF.Functions.WebSearchToTsQuery("simple", paging.SearchTerms)))
				.Select(g => new GameListModel
				{
					Id = g.Id,
					DisplayName = g.DisplayName,
					Systems = g.GameVersions.Select(v => v.System!.Code).ToList()
				})
				.SortedPageOf(paging);
		}
		else
		{
			data = await db.Games
				.ForSystemCode(paging.SystemCode)
				.ForGenre(paging.Genre)
				.ForGroup(paging.Group)
				.Where(g => g.DisplayName.StartsWith(paging.Letter ?? ""))
				.Select(g => new GameListModel
				{
					Id = g.Id,
					DisplayName = g.DisplayName,
					Systems = g.GameVersions.Select(v => v.System!.Code).ToList()
				})
				.SortedPageOf(paging);
		}

		return new SystemPageOf<GameListModel>(data)
		{
			SystemCode = paging.SystemCode,
			Letter = paging.Letter,
			Genre = paging.Genre,
			Group = paging.Group,
			SearchTerms = paging.SearchTerms,
			PageSize = data.PageSize,
			CurrentPage = data.CurrentPage,
			RowCount = data.RowCount,
			Sort = data.Sort
		};
	}

	public class GameListRequest : PagingModel
	{
		public GameListRequest()
		{
			PageSize = 50;
			Sort = "Name";
		}

		public string? SystemCode { get; init; }

		public string? Letter { get; init; }

		public string? Genre { get; init; }

		public string? Group { get; init; }

		public string? SearchTerms { get; set; }
	}

	public class SystemPageOf<T>(IEnumerable<T> items) : PageOf<T>(items)
	{
		[Display(Name = "System")]
		public string? SystemCode { get; init; }

		[Display(Name = "Starts with")]
		public string? Letter { get; init; }
		public string? Genre { get; init; }
		public string? Group { get; init; }
		public string? SearchTerms { get; init; }

		public static new SystemPageOf<T> Empty() => new([]);
	}

	public class GameListModel
	{
		[Sortable]
		public int Id { get; init; }

		[Sortable]
		[Display(Name = "Name")]
		public string DisplayName { get; init; } = "";

		public List<string> Systems { get; init; } = [];

		// Dummy to generate column header
		[Display(Name = "Actions")]
		public object? Actions { get; init; }
	}
}
