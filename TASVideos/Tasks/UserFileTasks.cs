using System;
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

		public async Task<UserMovieListViewModel> GetLatest(int count)
		{
			var query = _db.UserFiles
				.Include(userFile => userFile.Author)
				.Where(userFile => !userFile.Hidden)
				.OrderByDescending(userFile => userFile.UploadTimestamp)
				.Take(count);

			var result = await query.ToListAsync();

			return new UserMovieListViewModel
			{
				Entries = result.Select(userFile => new UserMovieListViewModel.Entry
				{
					Author = userFile.Author.UserName,
					FileName = userFile.FileName,
					Id = userFile.Id,
					Title = userFile.Title,
					Uploaded = userFile.UploadTimestamp,
				})
			};
		}

		/// <summary>
		/// Returns the info for the files uploaded by the given user
		/// </summary>
		public async Task<UserFileUserIndexViewModel> GetUserIndex(int userId, bool includeHidden)
		{
			var query = _db.UserFiles
				.Include(userFile => userFile.Author)
				.Include(userFile => userFile.Game)
				.Include(userFile => userFile.System)
				.Where(userFile => userFile.Author.Id == userId);

			if (!includeHidden)
			{
				query = query.Where(userFile => !userFile.Hidden);
			}
				

			var result = await query.ToListAsync();

			return new UserFileUserIndexViewModel
			{
				Files = result.Select(ToViewModel)
			};
		}

		/// <summary>
		/// Returns the info for the user file with the given id, or null if no such file was found.
		/// </summary>
		public async Task<UserFileViewModel> GetInfo(long id)
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
			var file = await _db.UserFiles.SingleOrDefaultAsync(userFile => userFile.Id == id);
			file.Views++;
			await _db.SaveChangesAsync();
		}

		public async Task IncrementDownloadCount(long id)
		{
			// TODO: Perhaps execute SQL instead?
			var file = await _db.UserFiles.SingleOrDefaultAsync(userFile => userFile.Id == id);
			file.Downloads++;
			await _db.SaveChangesAsync();
		}

		public async Task<UserFileIndexViewModel> GetIndex()
		{
			var model = new UserFileIndexViewModel();
			model.UsersWithMovies = await _db.UserFiles
				.GroupBy(gkey => gkey.Author.UserName, gvalue => gvalue.UploadTimestamp )
				.Select(uf => new UserFileIndexViewModel.UserWithMovie
				{
					UserName = uf.Key,
					Latest = uf.Max()
				})
				.ToListAsync();
			
			return model;
		}

		/// <summary>
		/// Returns the contents of the user file with the given id, or null if no such file was
		/// found.
		/// </summary>
		public async Task<UserFileDataViewModel> GetContents(long id)
		{
			var file = await _db.UserFiles
				.Where(userFile => userFile.Id == id)
				.Select(userFile => new UserFileDataViewModel
				{
					AuthorId = userFile.AuthorId,
					Content = userFile.Content,
					FileName = userFile.FileName,
					FileType = userFile.Type,
					Hidden = userFile.Hidden,
				})
				.SingleOrDefaultAsync();

			return file;
		}

		private static UserFileViewModel ToViewModel(UserFile file)
		{
			var model = file.Class == UserFileClass.Movie
				? new UserMovieViewModel()
				: new UserFileViewModel();

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

			if (model is UserMovieViewModel movie)
			{
				movie.Frames = file.Frames;
				movie.Length = TimeSpan.FromSeconds((double)file.Length);
				movie.Rerecords = file.Rerecords;
			}

			return model;
		}
	}
}
