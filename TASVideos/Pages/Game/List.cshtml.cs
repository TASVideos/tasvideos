using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewFeatures;

using TASVideos.Data;
using TASVideos.Data.Entity;
using TASVideos.Models;
using TASVideos.Tasks;

namespace TASVideos.Pages.Game
{
	[RequirePermission(PermissionTo.CatalogMovies)]
	public class ListModel : BasePageModel
	{
		private readonly CatalogTasks _catalogTasks;
		private readonly PlatformTasks _platformTasks;

		public ListModel(
			CatalogTasks catalogTasks,
			PlatformTasks platformTasks,
			UserTasks userTasks)
			: base(userTasks)
		{
			_catalogTasks = catalogTasks;
			_platformTasks = platformTasks;
		}

		[FromQuery]
		public GameListRequest Search { get; set; }

		public SystemPageOf<GameListModel> Games { get; set; }

		public async Task OnGet()
		{
			Games = _catalogTasks.GetPageOfGames(Search);
			var systems = (await _platformTasks.GetGameSystemDropdownList()).ToList();
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
