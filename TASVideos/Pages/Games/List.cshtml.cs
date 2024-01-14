using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using TASVideos.Core;
using TASVideos.Data;
using TASVideos.Data.Entity.Game;
using TASVideos.Pages.Games.Models;

namespace TASVideos.Pages.Games;

public class ListModel : BasePageModel
{
	private readonly ApplicationDbContext _db;

	public ListModel(
		ApplicationDbContext db)
	{
		_db = db;
	}

	[FromQuery]
	[StringLength(50, MinimumLength = 3)]
	[Display(Name = "Search")]
	public string? SearchTerms { get; set; }

	[FromQuery]
	public GameListRequest Search { get; set; } = new();

	public SystemPageOf<GameListModel> Games { get; set; } = SystemPageOf<GameListModel>.Empty();

	public List<SelectListItem> SystemList { get; set; } = new();

	public List<SelectListItem> LetterList { get; set; } = new();

	public List<SelectListItem> GenreList { get; set; } = new();

	public List<SelectListItem> GroupList { get; set; } = new();

	public async Task OnGet()
	{
		if (ModelState.IsValid)
		{
			Search.SearchTerms = SearchTerms;
			Games = await GetPageOfGames(Search);
		}

		SystemList = await _db.GameSystems
			.OrderBy(s => s.Code)
			.ToDropdown()
			.ToListAsync();

		SystemList.Insert(0, new SelectListItem { Text = "Any", Value = "" });

		LetterList = await _db.Games
			.Select(g => g.DisplayName.Substring(0, 1))
			.Distinct()
			.OrderBy(l => l)
			.ToDropdown()
			.ToListAsync();

		LetterList.Insert(0, new SelectListItem { Text = "Any", Value = "" });

		GenreList = await _db.Genres
			.Select(g => g.DisplayName)
			.Distinct()
			.OrderBy(l => l)
			.ToDropdown()
			.ToListAsync();

		GenreList.Insert(0, new SelectListItem { Text = "Any", Value = "" });

		GroupList = await _db.GameGroups
			.Select(g => g.Name)
			.Distinct()
			.OrderBy(l => l)
			.ToDropdown()
			.ToListAsync();

		GroupList.Insert(0, new SelectListItem { Text = "Any", Value = "" });
	}

	public async Task<IActionResult> OnGetFrameRateDropDownForSystem(int systemId, bool includeEmpty)
	{
		var items = await _db.GameSystemFrameRates
			.ForSystem(systemId)
			.ToDropDown()
			.ToListAsync();

		if (includeEmpty)
		{
			items = UiDefaults.DefaultEntry.Concat(items).ToList();
		}

		return new PartialViewResult
		{
			ViewName = "_DropdownItems",
			ViewData = new ViewDataDictionary<IEnumerable<SelectListItem>>(ViewData, items)
		};
	}

	public async Task<IActionResult> OnGetGameDropDownForSystem(int systemId, bool includeEmpty)
	{
		var items = await _db.Games
			.ForSystem(systemId)
			.OrderBy(g => g.DisplayName)
			.ToDropDown()
			.ToListAsync();

		if (includeEmpty)
		{
			items = UiDefaults.DefaultEntry.Concat(items).ToList();
		}

		return new PartialViewResult
		{
			ViewName = "_DropdownItems",
			ViewData = new ViewDataDictionary<IEnumerable<SelectListItem>>(ViewData, items)
		};
	}

	public async Task<IActionResult> OnGetVersionDropDownForGame(int gameId, int systemId, bool includeEmpty)
	{
		var items = await _db.GameVersions
			.ForGame(gameId)
			.ForSystem(systemId)
			.OrderBy(r => r.Name)
			.Select(r => new SelectListItem
			{
				Value = r.Id.ToString(),
				Text = r.Name
			})
			.ToListAsync();

		if (includeEmpty)
		{
			items = UiDefaults.DefaultEntry.Concat(items).ToList();
		}

		return new PartialViewResult
		{
			ViewName = "_DropdownItems",
			ViewData = new ViewDataDictionary<IEnumerable<SelectListItem>>(ViewData, items)
		};
	}

	public async Task<IActionResult> OnGetGameGoalDropDownForGame(int gameId, bool includeEmpty)
	{
		var items = await _db.GameGoals
			.Where(gg => gg.GameId == gameId)
			.OrderBy(gg => gg.DisplayName)
			.Select(gg => new SelectListItem
			{
				Value = gg.Id.ToString(),
				Text = gg.DisplayName
			})
			.ToListAsync();

		if (includeEmpty)
		{
			items = UiDefaults.DefaultEntry.Concat(items).ToList();
		}

		return new PartialViewResult
		{
			ViewName = "_DropdownItems",
			ViewData = new ViewDataDictionary<IEnumerable<SelectListItem>>(ViewData, items)
		};
	}

	private async Task<SystemPageOf<GameListModel>> GetPageOfGames(GameListRequest paging)
	{
		PageOf<GameListModel> data;
		if (!string.IsNullOrWhiteSpace(paging.SearchTerms))
		{
			_db.Database.SetCommandTimeout(TimeSpan.FromSeconds(30));
			data = await _db.Games
				.ForSystemCode(paging.SystemCode)
				.ForGenre(paging.Genre)
				.ForGroup(paging.Group)
				.Where(g => g.DisplayName.StartsWith(paging.Letter ?? ""))
				.Where(g => EF.Functions.ToTsVector("simple", g.DisplayName + " || " + g.Aliases + " || " + g.Abbreviation).Matches(EF.Functions.WebSearchToTsQuery("simple", paging.SearchTerms)))
				.Select(g => new GameListModel
				{
					Id = g.Id,
					DisplayName = g.DisplayName,
					Systems = g.GameVersions.Select(v => v.System!.Code)
				})
				.SortedPageOf(paging);
		}
		else
		{
			data = await _db.Games
				.ForSystemCode(paging.SystemCode)
				.ForGenre(paging.Genre)
				.ForGroup(paging.Group)
				.Where(g => g.DisplayName.StartsWith(paging.Letter ?? ""))
				.Select(g => new GameListModel
				{
					Id = g.Id,
					DisplayName = g.DisplayName,
					Systems = g.GameVersions.Select(v => v.System!.Code)
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
}
