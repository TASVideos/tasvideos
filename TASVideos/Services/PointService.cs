using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using TASVideos.Data;

namespace TASVideos.Services
{
    public interface IPointsService
	{
		/// <summary>
		/// Returns the player point calculation for the user with the given id
		/// </summary>
		/// <exception>If a user with the given id does not exist</exception>
		Task<int> CalculatePointsForUser(int id);
	}

	public class PointsService : IPointsService
    {
		private readonly ApplicationDbContext _db;
		private readonly ICacheService _cache;

		public PointsService(
			ApplicationDbContext db,
			ICacheService cache)
		{
			_db = db;
			_cache = cache;
		}


		public async Task<int> CalculatePointsForUser(int id)
		{
			var user = await _db.Users.SingleAsync(u => u.Id == id);
			return  new System.Random(DateTime.Now.Millisecond).Next(0, 10000);
		}
    }
}
