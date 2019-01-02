using System.Linq;
using System.Threading.Tasks;

using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

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
	}
}
