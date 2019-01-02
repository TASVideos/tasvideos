using System.Threading.Tasks;

using Microsoft.AspNetCore.Mvc;

using TASVideos.Data.Entity;
using TASVideos.Models;
using TASVideos.Tasks;

namespace TASVideos.Pages.Game.Rom
{
	[RequirePermission(PermissionTo.CatalogMovies)]
	public class ListModel : BasePageModel
	{
		private readonly CatalogTasks _catalogTasks;

		public ListModel(
			CatalogTasks catalogTasks,
			UserTasks userTasks)
			: base(userTasks)
		{
			_catalogTasks = catalogTasks;
		}

		[FromRoute]
		public int GameId { get; set; }

		public RomListModel Roms { get; set; }

		public async Task<IActionResult> OnGet()
		{
			Roms = await _catalogTasks.GetRomsForGame(GameId);
			if (Roms == null)
			{
				return NotFound();
			}

			return Page();
		}
	}
}
