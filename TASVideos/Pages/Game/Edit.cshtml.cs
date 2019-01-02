using System.Threading.Tasks;

using Microsoft.AspNetCore.Mvc;

using TASVideos.Data.Entity;
using TASVideos.Models;
using TASVideos.Tasks;

namespace TASVideos.Pages.Game
{
	[RequirePermission(PermissionTo.CatalogMovies)]
	public class EditModel : BasePageModel
	{
		private readonly CatalogTasks _catalogTasks;
		private readonly PlatformTasks _platformTasks;

		public EditModel(
			CatalogTasks catalogTasks,
			PlatformTasks platformTasks,
			UserTasks userTasks)
			: base(userTasks)
		{
			_catalogTasks = catalogTasks;
			_platformTasks = platformTasks;
		}

		[FromRoute]
		public int? Id { get; set; }

		[BindProperty]
		public GameEditModel Game { get; set; }

		public async Task<IActionResult> OnGet()
		{
			Game = Id.HasValue
				? (await _catalogTasks.GetGameForEdit(Id.Value))
				: new GameEditModel();

			if (Game == null)
			{
				return NotFound();
			}

			Game.AvailableSystems = await _platformTasks.GetGameSystemDropdownList();
			return Page();
		}

		public async Task<IActionResult> OnPost()
		{
			
			if (!ModelState.IsValid)
			{
				Game.AvailableSystems = await _platformTasks.GetGameSystemDropdownList();
				return Page();
			}

			await _catalogTasks.AddUpdateGame(Id, Game);
			return RedirectToPage("List");
		}

		public async Task<IActionResult> OnGetDelete()
		{
			if (Id == null)
			{
				return NotFound();
			}

			var result = await _catalogTasks.DeleteGame(Id.Value);
			if (result)
			{
				return RedirectToPage("List");
			}

			return BadRequest($"Unable to delete Game {Id}, game is used by a publication or submission");
		}
	}
}
