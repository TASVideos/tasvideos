using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using TASVideos.Constants;
using TASVideos.Data;
using TASVideos.Data.Entity.Game;
using TASVideos.Models;

namespace TASVideos.Tasks
{
	public class CatalogTasks
	{
		private readonly ApplicationDbContext _db;
		private readonly IMapper _mapper;

		public CatalogTasks(ApplicationDbContext db, IMapper mapper)
		{
			_db = db;
			_mapper = mapper;
		}

		public SystemPageOf<GameListModel> GetPageOfGames(GameListRequest paging)
		{
			var query = !string.IsNullOrWhiteSpace(paging.SystemCode)
				? _db.Games.Where(g => g.System.Code == paging.SystemCode)
				: _db.Games;

			var data = query
				.Select(g => new GameListModel
				{
					Id = g.Id,
					DisplayName = g.DisplayName,
					SystemCode = g.System.Code
				})
				.SortBy(paging)
				.SortedPageOf(_db, paging);

			return new SystemPageOf<GameListModel>(data)
			{
				SystemCode = paging.SystemCode,
				PageSize = data.PageSize,
				CurrentPage = data.CurrentPage,
				RowCount = data.RowCount,
				SortDescending = data.SortDescending,
				SortBy = data.SortBy
			};
		}

		public async Task AddUpdateGame(int? id, GameEditModel model)
		{
			Game game;
			if (id.HasValue)
			{
				game = await _db.Games.SingleAsync(g => g.Id == id.Value);
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
			bool canDelete = !await _db.Submissions.AnyAsync(s => s.Game.Id == id)
				&& !await _db.Publications.AnyAsync(p => p.Game.Id == id);

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
			return await _db.Games
				.Where(g => g.Id == gameId)
				.Select(g => new RomListModel
				{
					GameDisplayName = g.DisplayName,
					SystemCode = g.System.Code,
					Roms = g.Roms
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
					.ToList()
				})
				.SingleOrDefaultAsync();
		}

		public async Task<RomEditModel> GetRomForEdit(int gameId, int? romId)
		{
			using (_db.Database.BeginTransactionAsync())
			{
				var game = await _db.Games
					.Include(g => g.System)
					.SingleOrDefaultAsync(g => g.Id == gameId);

				if (game == null)
				{
					return null;
				}

				var model = romId.HasValue
					? await _db.Roms
						.Where(r => r.Id == romId && r.Game.Id == gameId)
						.ProjectTo<RomEditModel>()
						.SingleAsync()
					: new RomEditModel();

				model.GameName = game.DisplayName;
				model.SystemCode = game.System.Code;
				if (romId.HasValue)
				{
					model.CanDelete = !await _db.Submissions.AnyAsync(s => s.Rom.Id == romId.Value)
						&& !await _db.Publications.AnyAsync(p => p.Rom.Id == romId.Value);
				}

				return model;
			}
		}

		public async Task AddUpdateRom(int? id, int gameId, RomEditModel model)
		{
			GameRom rom;
			if (id.HasValue)
			{
				rom = await _db.Roms.SingleAsync(r => r.Id == id.Value);
				_mapper.Map(model, rom);
			}
			else
			{
				rom = _mapper.Map<GameRom>(model);
				rom.Game = await _db.Games.SingleAsync(g => g.Id == gameId);
				_db.Roms.Add(rom);
			}

			await _db.SaveChangesAsync();
		}

		public async Task<bool> DeleteRom(int id)
		{
			bool canDelete = !await _db.Submissions.AnyAsync(s => s.Rom.Id == id)
				&& !await _db.Publications.AnyAsync(p => p.Rom.Id == id);

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
				.ForSystem(systemId)
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
				.ForSystem(systemId)
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
