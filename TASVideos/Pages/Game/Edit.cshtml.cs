using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using AutoMapper;
using AutoMapper.QueryableExtensions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

using TASVideos.Data;
using TASVideos.Data.Constants;
using TASVideos.Data.Entity;
using TASVideos.Extensions;
using TASVideos.Pages.Game.Model;

namespace TASVideos.Pages.Game
{
	[RequirePermission(PermissionTo.CatalogMovies)]
	public class EditModel : BasePageModel
	{
		private readonly ApplicationDbContext _db;
		private readonly IMapper _mapper;

		public EditModel(
			ApplicationDbContext db,
			IMapper mapper)
		{
			_db = db;
			_mapper = mapper;
		}

		[TempData]
		public string Message { get; set; }

		[TempData]
		public string MessageType { get; set; }

		[FromRoute]
		public int? Id { get; set; }

		[BindProperty]
		public GameEditModel Game { get; set; } = new GameEditModel();

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

			try
			{
				MessageType = Styles.Success;
				Message = "Game successfully updated.";
				await _db.SaveChangesAsync();
			}
			catch (DbUpdateConcurrencyException)
			{
				MessageType = Styles.Danger;
				Message = $"Unable to update Game {Id}, the game may have already been updated, or the game no longer exists.";
			}
			
			return RedirectToPage("List");
		}

		public async Task<IActionResult> OnGetDelete()
		{
			if (!Id.HasValue)
			{
				return NotFound();
			}

			if (!await CanBeDeleted())
			{
				Message = $"Unable to delete Game {Id}, game is used by a publication or submission.";
				MessageType = Styles.Danger;
				return RedirectToPage("List");
			}

			try
			{
				_db.Games.Attach(new Data.Entity.Game.Game { Id = Id ?? 0 }).State = EntityState.Deleted;
				MessageType = Styles.Success;
				Message = $"Game {Id}, deleted successfully.";
				await _db.SaveChangesAsync();
			}
			catch (DbUpdateConcurrencyException)
			{
				MessageType = Styles.Danger;
				Message = $"Unable to delete Game {Id}, the game may have already been deleted or updated.";
			}

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
