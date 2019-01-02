using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

using TASVideos.Data.Entity;
using TASVideos.Data.Entity.Game;
using TASVideos.Models;
using TASVideos.Tasks;

namespace TASVideos.Pages.Game.Rom
{
	[RequirePermission(PermissionTo.CatalogMovies)]
	public class EditModel : BasePageModel
	{
		private static readonly IEnumerable<SelectListItem> RomTypes = Enum
			.GetValues(typeof(RomTypes))
			.Cast<RomTypes>()
			.Select(r => new SelectListItem
			{
				Text = r.ToString(),
				Value = ((int)r).ToString()
			});

		private readonly CatalogTasks _catalogTasks;

		public EditModel(
			CatalogTasks catalogTasks,
			UserTasks userTasks)
			: base(userTasks)
		{
			_catalogTasks = catalogTasks;
		}

		[FromRoute]
		public int GameId { get; set; }

		[FromRoute]
		public int? Id { get; set; }

		[BindProperty]
		public RomEditModel Rom { get; set; }

		public IEnumerable<SelectListItem> AvailableRomTypes => RomTypes;

		public async Task<IActionResult> OnGet()
		{
			Rom = await _catalogTasks.GetRomForEdit(GameId, Id);
			if (Rom == null)
			{
				return NotFound();
			}

			return Page();
		}

		public async Task<IActionResult> OnPost()
		{
			if (!ModelState.IsValid)
			{
				return Page();
			}

			await _catalogTasks.AddUpdateRom(Id, GameId, Rom);
			return RedirectToPage("List", new { gameId = GameId });
		}

		public async Task<ActionResult> OnGetDelete()
		{
			if (!Id.HasValue)
			{
				return NotFound();
			}

			var result = await _catalogTasks.DeleteRom(Id.Value);
			if (result)
			{
				return RedirectToPage("List", new { GameId });
			}

			return BadRequest($"Unable to delete Rom {Id}, rom is used by a publication or submission");
		}
	}
}
