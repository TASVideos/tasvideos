using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

using AutoMapper;
using AutoMapper.QueryableExtensions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

using TASVideos.Data;
using TASVideos.Data.Entity;
using TASVideos.Data.Entity.Game;
using TASVideos.Extensions;
using TASVideos.Pages.Games.Models;
using TASVideos.Services;

namespace TASVideos.Pages.Games
{
	[RequirePermission(PermissionTo.CatalogMovies)]
	public class EditModel : BasePageModel
	{
		private readonly ApplicationDbContext _db;
		private readonly IWikiPages _wikiPages;
		private readonly IMapper _mapper;

		public EditModel(
			ApplicationDbContext db,
			IWikiPages wikiPages,
			IMapper mapper)
		{
			_db = db;
			_wikiPages = wikiPages;
			_mapper = mapper;
		}

		[TempData]
		public string Message { get; set; }

		[TempData]
		public string MessageType { get; set; }

		[FromRoute]
		public int? Id { get; set; }

		[FromQuery]
		public string ReturnUrl { get; set; }

		[FromQuery]
		public int? SystemId { get; set; }

		[BindProperty]
		public GameEditModel Game { get; set; } = new GameEditModel();

		public bool CanDelete { get; set; }
		public IEnumerable<SelectListItem> AvailableSystems { get; set; } = new List<SelectListItem>();

		[Display(Name = "Available Genres")]
		public IEnumerable<SelectListItem> AvailableGenres { get; set; } = new List<SelectListItem>();

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
			else if (SystemId.HasValue)
			{
				var systemCode = await _db.GameSystems
					.Where(s => s.Id == SystemId)
					.Select(s => s.Code)
					.SingleOrDefaultAsync();
				if (systemCode != null)
				{
					Game.SystemCode = systemCode;
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

			if (!string.IsNullOrEmpty(Game.GameResourcesPage))
			{
				var page = _wikiPages.Page(Game.GameResourcesPage);
				if (page == null)
				{
					ModelState.AddModelError($"{nameof(Game)}.{nameof(Game.GameResourcesPage)}", $"Page {Game.GameResourcesPage} not found");
					await Initialize();
					return Page();
				}
			}

			Game game;
			if (Id.HasValue)
			{
				game = await _db.Games
					.Include(g => g.GameGenres)
					.SingleOrDefaultAsync(g => g.Id == Id.Value);
				if (game == null)
				{
					return NotFound();
				}

				game.GameGenres.Clear();
				_mapper.Map(Game, game);
			}
			else
			{
				game = _mapper.Map<Game>(Game);
				_db.Games.Add(game);
			}

			game.GameResourcesPage = Game.GameResourcesPage;
			game.System = await _db.GameSystems
				.SingleOrDefaultAsync(s => s.Code == Game.SystemCode);

			if (game.System == null)
			{
				return BadRequest();
			}

			foreach (var genre in Game.Genres)
			{
				game.GameGenres.Add(new GameGenre
				{
					Game = game,
					GenreId = genre
				});
			}

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

			await _db.SaveChangesAsync();
			
			return string.IsNullOrWhiteSpace(ReturnUrl)
				? RedirectToPage("List")
				: RedirectToLocal(ReturnUrl);
		}

		public async Task<IActionResult> OnPostDelete()
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
				_db.Games.Attach(new Game { Id = Id ?? 0 }).State = EntityState.Deleted;
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

			AvailableGenres = await _db.Genres
				.ToDropdown()
				.ToListAsync();

			CanDelete = await CanBeDeleted();
		}

		private async Task<bool> CanBeDeleted()
		{
			return Id > 0
				&& !await _db.Submissions.AnyAsync(s => s.Game.Id == Id)
				&& !await _db.Publications.AnyAsync(p => p.Game.Id == Id);
		}
	}
}
