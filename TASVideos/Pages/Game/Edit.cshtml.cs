using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using AutoMapper.QueryableExtensions;

using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

using TASVideos.Data;
using TASVideos.Data.Entity;
using TASVideos.Extensions;
using TASVideos.Models;
using TASVideos.Tasks;

namespace TASVideos.Pages.Game
{
	[RequirePermission(PermissionTo.CatalogMovies)]
	public class EditModel : BasePageModel
	{
		private readonly CatalogTasks _catalogTasks;
		private readonly ApplicationDbContext _db;

		public EditModel(
			ApplicationDbContext db,
			CatalogTasks catalogTasks,
			UserTasks userTasks)
			: base(userTasks)
		{
			_catalogTasks = catalogTasks;
			_db = db;
		}

		[FromRoute]
		public int? Id { get; set; }

		[BindProperty]
		public GameEditModel Game { get; set; }

		public bool CanDelete { get; set; }
		public IEnumerable<SelectListItem> AvailableSystems { get; set; } = new List<SelectListItem>();

		public async Task<IActionResult> OnGet()
		{
			if (Id.HasValue)
			{
				Game = await _db.Games
					.Where(g => g.Id == Id)
					.ProjectTo<GameEditModel>()
					.SingleOrDefaultAsync();

				if (Game == null)
				{
					return NotFound();
				}
			}
			else
			{
				Game = new GameEditModel();
			}

			await Initialize();
			return Page();
		}

		public async Task<IActionResult> OnPost()
		{
			if (!ModelState.IsValid)
			{
				await Initialize();
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

		private async Task Initialize()
		{
			AvailableSystems = await _db.GameSystems
					.ToDropdown()
					.ToListAsync();

				CanDelete = !await _db.Submissions.AnyAsync(s => s.Game.Id == Id)
							&& !await _db.Publications.AnyAsync(p => p.Game.Id == Id);
		}
	}
}
