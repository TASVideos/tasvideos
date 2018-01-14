using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using Microsoft.EntityFrameworkCore;
using TASVideos.Data;
using TASVideos.Data.Entity.Game;
using TASVideos.Models;

namespace TASVideos.Tasks
{
	using System.Collections.Generic;

	using Microsoft.AspNetCore.Mvc.Rendering;

	using TASVideos.Constants;

	public class CatalogTasks
	{
		private readonly ApplicationDbContext _db;
		private readonly IMapper _mapper;

		public CatalogTasks(ApplicationDbContext db, IMapper mapper)
		{
			_db = db;
			_mapper = mapper;
		}

		public PageOf<GameListModel> GetPageOfGames(PagedModel paging) // TODO: ability to filter by system
		{
			var data = _db.Games
				.Include(g => g.System)
				.Select(g => new GameListModel
				{
					Id = g.Id,
					DisplayName = g.DisplayName,
					SystemCode = g.System.Code
				})
				.SortedPageOf(_db, paging);

			return data;
		}

		public async Task<GameEditModel> GetGameForEdit(int gameId)
		{
			var model = await _db.Games
				.Where(g => g.Id == gameId)
				.ProjectTo<GameEditModel>()
				.SingleAsync();

			model.CanDelete = !(await _db.Submissions.AnyAsync(s => s.Game.Id == model.Id))
							&& !(await _db.Publications.AnyAsync(p => p.Game.Id == model.Id));

			return model;
		}

		public async Task AddUpdateGame(GameEditModel model)
		{
			Game game;
			if (model.Id.HasValue)
			{
				game = await _db.Games.SingleAsync(g => g.Id == model.Id.Value);
				_mapper.Map(model, game);
			}
			else
			{
				game = _mapper.Map<Game>(model);
				_db.Games.Add(game);
			}

			game.System = await _db.GameSystems.SingleAsync(s => s.Code == model.SystemCode);
			await _db.SaveChangesAsync();
		}

		public async Task<bool> DeleteGame(int id)
		{
			bool canDelete = !(await _db.Submissions.AnyAsync(s => s.Game.Id == id))
							&& !(await _db.Publications.AnyAsync(p => p.Game.Id == id));

			if (!canDelete)
			{
				return false;
			}

			var game = await _db.Games.SingleAsync(r => r.Id == id);
			_db.Games.Remove(game);
			await _db.SaveChangesAsync();
			return true;
		}

		public async Task<RomListModel> GetRomsForGame(int gameId)
		{
			var game = await _db.Games
				.Include(g => g.System)
				.SingleAsync(g => g.Id == gameId);

			var data = new RomListModel
			{
				GameId = gameId,
				GameDisplayName = game.DisplayName,
				SystemCode = game.System.Code,
				Roms = await _db.Roms
					.Where(r => r.Game.Id == gameId)
					.Select(r => new RomListModel.RomEntry
					{
						Id = r.Id,
						DisplayName = r.Name,
						Md5 = r.Md5,
						Sha1 = r.Sha1,
						Version = r.Version,
						Region = r.Region,
						RomType = r.Type
					})
					.ToListAsync()
			};

			return data;
		}

		public async Task<RomEditModel> GetRomForEdit(int gameId, int? romId)
		{
			using (_db.Database.BeginTransactionAsync())
			{
				var game = await _db.Games
					.Include(g => g.System)
					.SingleAsync(g => g.Id == gameId);

				var model = romId.HasValue
					? await _db.Roms
						.Where(r => r.Id == romId && r.Game.Id == gameId)
						.ProjectTo<RomEditModel>()
						.SingleAsync()
					: new RomEditModel();

				model.GameName = game.DisplayName;
				model.GameId = game.Id;
				model.SystemCode = game.System.Code;
				if (romId.HasValue)
				{
					model.CanDelete = !(await _db.Submissions.AnyAsync(s => s.Rom.Id == model.Id))
						&& !(await _db.Publications.AnyAsync(p => p.Rom.Id == model.Id));
				}

				return model;
			}
		}

		public async Task AddUpdateRom(RomEditModel model)
		{
			GameRom rom;
			if (model.Id.HasValue)
			{
				rom = await _db.Roms.SingleAsync(r => r.Id == model.Id.Value);
				_mapper.Map(model, rom);
			}
			else
			{
				rom = _mapper.Map<GameRom>(model);
				rom.Game = await _db.Games.SingleAsync(g => g.Id == model.GameId);
				rom.System = await _db.GameSystems.SingleAsync(s => s.Code == model.SystemCode);
				_db.Roms.Add(rom);
			}

			await _db.SaveChangesAsync();
		}

		public async Task<bool> DeleteRom(int id)
		{
			bool canDelete = !(await _db.Submissions.AnyAsync(s => s.Rom.Id == id))
				&& !(await _db.Publications.AnyAsync(p => p.Rom.Id == id));

			if (!canDelete)
			{
				return false;
			}

			var rom = await _db.Roms.SingleAsync(r => r.Id == id);
			_db.Roms.Remove(rom);
			await _db.SaveChangesAsync();
			return true;
		}

		public async Task<IEnumerable<SelectListItem>> GetFrameRateDropDownForSystem(int systemId, bool includeEmpty)
		{
			var items = (await _db.GameSystemFrameRates
				.Where(g => g.GameSystemId == systemId)
				.Select(g => new SelectListItem
				{
					Value = g.Id.ToString(),
					Text = g.RegionCode + " " + g.FrameRate
				})
				.ToListAsync())
				.OrderBy(r => r.Value);

			return includeEmpty
				? UiDefaults.DefaultEntry.Concat(items)
				: items;
		}

		public async Task<IEnumerable<SelectListItem>> GetGameDropDownForSystem(int systemId, bool includeEmpty)
		{
			var items = (await _db.Games
				.Where(g => g.SystemId == systemId)
				.Select(g => new SelectListItem
				{
					Value = g.Id.ToString(),
					Text = g.GoodName
				})
				.ToListAsync())
				.OrderBy(r => r.Value);

			return includeEmpty
				? UiDefaults.DefaultEntry.Concat(items)
				: items;
		}

		public async Task<IEnumerable<SelectListItem>> GetRomDropDownForGame(int gameId, bool includeEmpty)
		{
			var items = (await _db.Roms
				.Where(r => r.GameId == gameId)
				.Select(r => new SelectListItem
				{
					Value = r.Id.ToString(),
					Text = r.Name
				})
				.ToListAsync())
				.OrderBy(r => r.Value);

			return includeEmpty
				? UiDefaults.DefaultEntry.Concat(items)
				: items;
		}
	}
}
