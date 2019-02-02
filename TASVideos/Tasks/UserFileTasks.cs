using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

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

		public UserFileTasks(ApplicationDbContext db)
		{
			_db = db;
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

		public async Task IncrementViewCount(long id)
		{
			// TODO: Perhaps execute SQL instead?
			// TODO: handle concurrency exceptions
			var file = await _db.UserFiles.SingleOrDefaultAsync(userFile => userFile.Id == id);
			file.Views++;
			await _db.SaveChangesAsync();
		}
	}
}
