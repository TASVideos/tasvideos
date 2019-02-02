using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using AutoMapper;
using AutoMapper.QueryableExtensions;

using Microsoft.EntityFrameworkCore;

using TASVideos.Data;
using TASVideos.Data.Entity;
using TASVideos.Models;

namespace TASVideos.Tasks
{
	public class UserFileTasks
	{
		private readonly ApplicationDbContext _db;
		private readonly IMapper _mapper;

		public UserFileTasks(ApplicationDbContext db, IMapper mapper)
		{
			_db = db;
			_mapper = mapper;
		}

		/// <summary>
		/// Returns the info for the files uploaded by the given user
		/// </summary>
		public async Task<IEnumerable<UserFileModel>> GetUserIndex(string userName, bool includeHidden)
		{
			var query = _db.UserFiles
				.ForAuthor(userName);

			if (!includeHidden)
			{
				query = query.Where(userFile => !userFile.Hidden);
			}

			return await query
				.ProjectTo<UserFileModel>()
				.ToListAsync();
		}

		/// <summary>
		/// Returns the info for the user file with the given id, or null if no such file was found.
		/// </summary>
		public async Task<UserFileModel> GetInfo(long id)
		{
			return await _db.UserFiles
				.Where(userFile => userFile.Id == id)
				.ProjectTo<UserFileModel>()
				.SingleOrDefaultAsync();
		}

		public async Task IncrementViewCount(long id)
		{
			// TODO: Perhaps execute SQL instead?
			// TODO: handle concurrency exceptions
			var file = await _db.UserFiles.SingleOrDefaultAsync(userFile => userFile.Id == id);
			file.Views++;
			await _db.SaveChangesAsync();
		}

		public async Task<GameFileModel> GetFilesForGame(int gameId)
		{
			var game = await _db.Games
				.Include(g => g.System)
				.Include(g => g.UserFiles)
				.ThenInclude(u => u.Author)
				.SingleOrDefaultAsync(g => g.Id == gameId);

			if (game == null)
			{
				return null;
			}

			return new GameFileModel
			{
				GameId = game.Id,
				SystemCode = game.System.Code,
				GameName = game.DisplayName,
				Files = game.UserFiles
					.Where(uf => !uf.Hidden)
					.Select(_mapper.Map<UserFileModel>)
					.ToList()
			};
		}
	}
}
