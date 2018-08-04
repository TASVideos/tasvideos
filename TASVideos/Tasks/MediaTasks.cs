using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using TASVideos.Data;
using TASVideos.Data.Entity;
using TASVideos.Services.ExternalMediaPublisher;

namespace TASVideos.Tasks
{
    public class MediaTasks
    {
		private readonly ApplicationDbContext _db;

		public MediaTasks(ApplicationDbContext db)
		{
			_db = db;
		}

		public async Task<IEnumerable<MediaPost>> GetPosts(DateTime startDate, int limit)
		{
			return await _db.MediaPosts
				.Where(m => m.CreateTimeStamp >= startDate)
				.Where(m => m.Type != PostType.Critical.ToString()) // TODO: Permission check to see these
				.Where(m => m.Type != PostType.Administrative.ToString()) // TODO: Permission check to see these
				.Where(m => m.Type != PostType.Log.ToString()) // TODO: Permission check to see these (and/or a parameter)
				.OrderByDescending(m => m.CreateTimeStamp)
				.Take(limit)
				.ToListAsync();
				
		}
    }
}
