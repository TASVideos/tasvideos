using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.EntityFrameworkCore;

using TASVideos.Data;
using TASVideos.Data.Entity;
using TASVideos.Extensions;
using TASVideos.Models;
using TASVideos.Tasks;

namespace TASVideos.Pages.Game
{
	[RequirePermission(PermissionTo.CatalogMovies)]
	public class ListModel : BasePageModel
	{
		private readonly CatalogTasks _catalogTasks;
		private readonly ApplicationDbContext _db;

		public ListModel(
			CatalogTasks catalogTasks,
			ApplicationDbContext db,
			UserTasks userTasks)
			: base(userTasks)
		{
			_catalogTasks = catalogTasks;
			_db = db;
		}

		[TempData]
		public string Message { get; set; }

		public bool ShowMessage => !string.IsNullOrWhiteSpace(Message);

		[FromQuery]
		public GameListRequest Search { get; set; }

		public SystemPageOf<GameListModel> Games { get; set; }

		public async Task OnGet()
		{
			Games = _catalogTasks.GetPageOfGames(Search);
			var systems = await _db.GameSystems
				.ToDropdown()
				.ToListAsync();

			systems.Insert(0, new SelectListItem { Text = "All", Value = "" });

			ViewData["GameSystemList"] = systems;
		}

		public async Task<IActionResult> OnGetFrameRateDropDownForSystem(int systemId, bool includeEmpty)
		{
			var items = await _catalogTasks.GetFrameRateDropDownForSystem(systemId, includeEmpty);
			return new PartialViewResult
			{
				ViewName = "_DropdownItems",
				ViewData = new ViewDataDictionary<IEnumerable<SelectListItem>>(ViewData, items)
			};
		}

		public async Task<IActionResult> OnGetGameDropDownForSystem(int systemId, bool includeEmpty)
		{
			var items = await _catalogTasks.GetGameDropDownForSystem(systemId, includeEmpty);
			return new PartialViewResult
			{
				ViewName = "_DropdownItems",
				ViewData = new ViewDataDictionary<IEnumerable<SelectListItem>>(ViewData, items)
			};
		}

		public async Task<IActionResult> OnGetRomDropDownForGame(int gameId, bool includeEmpty)
		{
			var items = await _catalogTasks.GetRomDropDownForGame(gameId, includeEmpty);
			return new PartialViewResult
			{
				ViewName = "_DropdownItems",
				ViewData = new ViewDataDictionary<IEnumerable<SelectListItem>>(ViewData, items)
			};
		}
	}
}
