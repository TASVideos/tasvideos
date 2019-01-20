using System;
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

		private readonly ApplicationDbContext _db;
		private readonly IMapper _mapper;

		public EditModel(
			ApplicationDbContext db,
			IMapper mapper,
			UserTasks userTasks)
			: base(userTasks)
		{
			_db = db;
			_mapper = mapper;
		}

		[FromRoute]
		public int GameId { get; set; }

		[FromRoute]
		public int? Id { get; set; }

		[BindProperty]
		public RomEditModel Rom { get; set; } = new RomEditModel();

		[BindProperty]
		public string SystemCode { get; set; }

		[BindProperty]
		public string GameName { get; set; }

		public bool CanDelete { get; set; }
		public IEnumerable<SelectListItem> AvailableRomTypes => RomTypes;

		public async Task<IActionResult> OnGet()
		{
			if (!Id.HasValue)
			{
				return Page();
			}

			var game = await _db.Games
				.Include(g => g.System)
				.SingleOrDefaultAsync(g => g.Id == Id.Value);

			if (game == null)
			{
				return NotFound();
			}

			Rom = await _db.Roms
				.Where(r => r.Id == Id.Value && r.Game.Id == GameId)
				.ProjectTo<RomEditModel>()
				.SingleAsync();

			if (Rom == null)
			{
				return NotFound();
			}

			GameName = game.DisplayName;
			SystemCode = game.System.Code;

			CanDelete = await CanBeDeleted();

			return Page();
		}

		public async Task<IActionResult> OnPost()
		{
			if (!ModelState.IsValid)
			{
				CanDelete = await CanBeDeleted();
				return Page();
			}

			GameRom rom;
			if (Id.HasValue)
			{
				rom = await _db.Roms.SingleAsync(r => r.Id == Id.Value);
				_mapper.Map(Rom, rom);
			}
			else
			{
				rom = _mapper.Map<GameRom>(Rom);
				rom.Game = await _db.Games.SingleAsync(g => g.Id == GameId);
				_db.Roms.Add(rom);
			}

			await _db.SaveChangesAsync();

			return RedirectToPage("List", new { gameId = GameId });
		}

		public async Task<ActionResult> OnGetDelete()
		{
			if (!Id.HasValue)
			{
				return NotFound();
			}

			if (!await CanBeDeleted())
			{
				return RedirectToPage("List");
			}

			// TODO: catch concurrency exception
			_db.Roms.Attach(new GameRom { Id = Id ?? 0 }).State = EntityState.Deleted;
			await _db.SaveChangesAsync();
			return RedirectToPage("List");
		}

		private async Task<bool> CanBeDeleted()
		{
			return !await _db.Submissions.AnyAsync(s => s.Rom.Id == Id)
					&& !await _db.Publications.AnyAsync(p => p.Rom.Id == Id);
		}
	}
}
