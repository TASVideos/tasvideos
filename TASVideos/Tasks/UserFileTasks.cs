using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using Microsoft.EntityFrameworkCore;

using TASVideos.Data;
using TASVideos.Data.Entity;
using TASVideos.Models;

namespace TASVideos.Tasks
{
	public class UserFileTasks
	{
		private readonly ApplicationDbContext _db;

		public UserFileTasks(ApplicationDbContext db)
		{
			_db = db;
		}

		public async Task<IEnumerable<UserMovieListModel>> GetLatest(int count)
		{
			return await _db.UserFiles
				.Where(userFile => !userFile.Hidden)
				.OrderByDescending(userFile => userFile.UploadTimestamp)
				.Select(userFile => new UserMovieListModel
				{
					Author = userFile.Author.UserName,
					FileName = userFile.FileName,
					Id = userFile.Id,
					Title = userFile.Title,
					Uploaded = userFile.UploadTimestamp,
				})
				.Take(count)
				.ToListAsync();
		}

		/// <summary>
		/// Returns the info for the files uploaded by the given user
		/// </summary>
		public async Task<IEnumerable<UserFileModel>> GetUserIndex(string userName, bool includeHidden)
		{
			var query = _db.UserFiles
				.Include(userFile => userFile.Author)
				.Include(userFile => userFile.Game)
				.Include(userFile => userFile.System)
				.Where(userFile => userFile.Author.UserName == userName);

			if (!includeHidden)
			{
				query = query.Where(userFile => !userFile.Hidden);
			}

			var result = await query.ToListAsync();
			return result.Select(ToViewModel);
		}

		/// <summary>
		/// Returns the info for the user file with the given id, or null if no such file was found.
		/// </summary>
		public async Task<UserFileModel> GetInfo(long id)
		{
			var file = await _db.UserFiles
				.Include(userFile => userFile.Author)
				.Include(userFile => userFile.Game)
				.Include(userFile => userFile.System)
				.Where(userFile => userFile.Id == id)
				.SingleOrDefaultAsync();

			return file == null ? null : ToViewModel(file);
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
					.Select(ToViewModel)
					.ToList()
			};
		}

		private static UserFileModel ToViewModel(UserFile file)
		{
			var model = file.Class == UserFileClass.Movie
				? new UserMovieModel()
				: new UserFileModel();

			model.Author = file.Author.UserName;
			model.Description = file.Description;
			model.Downloads = file.Downloads;
			model.Id = file.Id;
			model.Title = file.Title;
			model.Uploaded = file.UploadTimestamp;
			model.Views = file.Views;
			model.Hidden = file.Hidden;
			model.FileName = file.FileName;
			model.FileSize = file.LogicalLength;
			model.GameId = file.Game?.Id;
			model.GameName = file.Game?.DisplayName;
			model.System = file.System?.DisplayName;

			if (model is UserMovieModel movie)
			{
				movie.Frames = file.Frames;
				movie.Length = TimeSpan.FromSeconds((double)file.Length);
				movie.Rerecords = file.Rerecords;
			}

			return model;
		}
	}
}
