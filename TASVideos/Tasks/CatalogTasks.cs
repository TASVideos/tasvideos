using System;
using System.Collections.Generic;
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
			return await _db.Games
				.Where(g => g.Id == gameId)
				.ProjectTo<GameEditModel>()
				.SingleAsync();
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
	}
}
