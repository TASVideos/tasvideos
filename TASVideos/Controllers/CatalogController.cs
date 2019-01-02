using System.Threading.Tasks;

using Microsoft.AspNetCore.Mvc;

using TASVideos.Data.Entity;
using TASVideos.Filter;
using TASVideos.Tasks;

namespace TASVideos.Controllers
{
	[RequirePermission(PermissionTo.CatalogMovies)]
	public class CatalogController : BaseController
	{
		private readonly CatalogTasks _catalogTasks;

		public CatalogController(
			CatalogTasks catalogTasks,
			UserTasks userTasks)
			: base(userTasks)
		{
			_catalogTasks = catalogTasks;
		}

		public async Task<IActionResult> FrameRateDropDownForSystem(int systemId, bool includeEmpty)
		{
			var model = await _catalogTasks.GetFrameRateDropDownForSystem(systemId, includeEmpty);
			return PartialView("_DropdownItems", model);
		}

		public async Task<IActionResult> GameDropDownForSystem(int systemId, bool includeEmpty)
		{
			var model = await _catalogTasks.GetGameDropDownForSystem(systemId, includeEmpty);
			return PartialView("_DropdownItems", model);
		}

		public async Task<IActionResult> RomDropDownForGame(int gameId, bool includeEmpty)
		{
			var model = await _catalogTasks.GetRomDropDownForGame(gameId, includeEmpty);
			return PartialView("_DropdownItems", model);
		}
	}
}
