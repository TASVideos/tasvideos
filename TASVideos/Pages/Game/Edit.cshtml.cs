using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using AutoMapper;
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
		private readonly IMapper _mapper;

		public EditModel(
			ApplicationDbContext db,
			CatalogTasks catalogTasks,
			IMapper mapper,
			UserTasks userTasks)
			: base(userTasks)
		{
			_catalogTasks = catalogTasks;
			_db = db;
			_mapper = mapper;
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

			Data.Entity.Game.Game game;
			if (Id.HasValue)
			{
				game = await _db.Games.SingleOrDefaultAsync(g => g.Id == Id.Value);
				if (game == null)
				{
					return NotFound();
				}

				_mapper.Map(Game, game);
			}
			else
			{
				game = _mapper.Map<Data.Entity.Game.Game>(Game);
				_db.Games.Add(game);
			}

			game.System = await _db.GameSystems
				.SingleAsync(s => s.Code == Game.SystemCode);

			await _db.SaveChangesAsync();
			return RedirectToPage("List");
		}

		public async Task<IActionResult> OnGetDelete()
		{
			if (Id == null)
			{
				return NotFound();
			}

			if (!await CanBeDeleted())
			{
				return BadRequest($"Unable to delete Game {Id}, game is used by a publication or submission");
			}

			var game = await _db.Games
				.SingleOrDefaultAsync(r => r.Id == Id);

			if (game == null)
			{
				return NotFound();
			}

			_db.Games.Remove(game);
			await _db.SaveChangesAsync();
			return RedirectToPage("List");
		}

		private async Task Initialize()
		{
			AvailableSystems = await _db.GameSystems
					.ToDropdown()
					.ToListAsync();

				CanDelete = await CanBeDeleted();
		}

		private async Task<bool> CanBeDeleted()
		{
			return !await _db.Submissions.AnyAsync(s => s.Game.Id == Id)
					&& !await _db.Publications.AnyAsync(p => p.Game.Id == Id);
		}
	}
}
