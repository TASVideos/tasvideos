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
	public class CatalogTasks
	{
		private readonly ApplicationDbContext _db;
		private readonly IMapper _mapper;

		public CatalogTasks(ApplicationDbContext db, IMapper mapper)
		{
			_db = db;
			_mapper = mapper;
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
	}
}
