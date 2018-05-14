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

		public async Task<UserFileUserIndexViewModel> GetUserIndex(User user)
		{
			var query = _db.UserFiles
				.Include(userFile => userFile.Author)
				.Where(userFile => userFile.Author.Id == user.Id);

			var result = await query.ToListAsync();

			return new UserFileUserIndexViewModel
			{
				Files = result.Select(ToViewModel)
			};
		}

		public async Task<UserFileViewModel> GetInfo(long id)
		{
			var file = await _db.UserFiles
				.Include(userFile => userFile.Author)
				.Where(userFile => userFile.Id == id)
				.SingleOrDefaultAsync();

			return ToViewModel(file);
		}

		public async Task<UserFileIndexViewModel> GetIndex()
		{
			throw new NotImplementedException();
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
