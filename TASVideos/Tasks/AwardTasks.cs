using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

using TASVideos.Data;
using TASVideos.Data.Entity.Awards;
using TASVideos.Models;
using TASVideos.Services;

namespace TASVideos.Tasks
{
	public class AwardTasks
	{
		private readonly ApplicationDbContext _db;
		private readonly ICacheService _cache;

		public AwardTasks(
			ApplicationDbContext db,
			ICacheService cache)
		{
			_db = db;
			_cache = cache;
		}

		/// <summary>
		/// Gets all awards for the user, or any movie for which the user is an author of
		/// </summary>
		public async Task<IEnumerable<AwardDisplayModel>> GetAllAwardsForUser(int userId)
		{
			// TODO: caching - cache all awards (for a really long time) then select the ones relevant to the user

			var userAwards = await _db.UserAwards
				.Where(ua => ua.UserId == userId)
				.Select(ua => new AwardDisplayModel
				{
					ShortName = ua.Award.ShortName,
					Description = ua.Award.Description,
					Year = ua.Year
				})
				.ToListAsync();

			return Enumerable.Empty<AwardDisplayModel>();
		}
	}
}
