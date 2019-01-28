using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.EntityFrameworkCore;

using TASVideos.Constants;
using TASVideos.Data;
using TASVideos.Data.Entity;
using TASVideos.Data.Entity.Game;
using TASVideos.Extensions;
using TASVideos.Pages.Game.Model;

namespace TASVideos.Pages.Game
{
	[RequirePermission(PermissionTo.CatalogMovies)]
	public class ListModel : BasePageModel
	{
		private readonly ApplicationDbContext _db;

		public ListModel(
			ApplicationDbContext db)
		{
			_db = db;
		}

		[TempData]
		public string Message { get; set; }

		[TempData]
		public string MessageType { get; set; }

		public bool ShowMessage => !string.IsNullOrWhiteSpace(Message);

		[FromQuery]
		public GameListRequest Search { get; set; }

		public SystemPageOf<GameListModel> Games { get; set; }

		public List<SelectListItem> SystemList { get; set; } = new List<SelectListItem>();

		public async Task OnGet()
		{
			Games = GetPageOfGames(Search);
			SystemList = await _db.GameSystems
				.ToDropdown()
				.ToListAsync();

			SystemList.Insert(0, new SelectListItem { Text = "All", Value = "" });
		}

		public async Task<IActionResult> OnGetFrameRateDropDownForSystem(int systemId, bool includeEmpty)
		{
			var items = await _db.GameSystemFrameRates
				.ForSystem(systemId)
				.OrderBy(fr => fr.Id)
				.Select(g => new SelectListItem
				{
					Value = g.Id.ToString(),
					Text = g.RegionCode + " " + g.FrameRate
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

		public async Task<IActionResult> OnGetGameDropDownForSystem(int systemId, bool includeEmpty)
		{
			var items = await _db.Games
				.OrderBy(g => g.Id)
				.ForSystem(systemId)
				.Select(g => new SelectListItem
				{
					Value = g.Id.ToString(),
					Text = g.GoodName
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

		public async Task<IActionResult> OnGetRomDropDownForGame(int gameId, bool includeEmpty)
		{
			var items = await _db.Roms
				.Where(r => r.GameId == gameId)
				.OrderBy(r => r.Id)
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

		private SystemPageOf<GameListModel> GetPageOfGames(GameListRequest paging)
		{
			var query = !string.IsNullOrWhiteSpace(paging.SystemCode)
				? _db.Games.Where(g => g.System.Code == paging.SystemCode)
				: _db.Games;

			var data = query
				.Select(g => new GameListModel
				{
					Id = g.Id,
					DisplayName = g.DisplayName,
					SystemCode = g.System.Code
				})
				.SortBy(paging)
				.SortedPageOf(_db, paging);

			return new SystemPageOf<GameListModel>(data)
			{
				SystemCode = paging.SystemCode,
				PageSize = data.PageSize,
				CurrentPage = data.CurrentPage,
				RowCount = data.RowCount,
				SortDescending = data.SortDescending,
				SortBy = data.SortBy
			};
		}
	}
}
