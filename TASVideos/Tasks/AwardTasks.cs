using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

using TASVideos.Data;
using TASVideos.Data.Constants;
using TASVideos.Data.Entity.Awards;
using TASVideos.Models;
using TASVideos.Services;

namespace TASVideos.Tasks
{
	public class AwardTasks
	{
		private const string AwardCacheKey = "AwardsCache";

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
			var allAwards = await AllAwardsCache();

			return allAwards
				.Where(a => a.UserIds.Contains(userId))
				.Select(ua => new AwardDisplayModel
				{
					ShortName = ua.ShortName,
					Description = ua.Description,
					Year = ua.Year
				})
				.ToList();
		}

		private class AwardDto
		{
			public string ShortName { get; set; }
			public string Description { get; set; }
			public int Year { get; set; }
			public AwardType Type { get; set; }
			public int? PublicationId { get; set; }
			public IEnumerable<int> UserIds { get; set; } = new HashSet<int>();
		}

		private async Task<IEnumerable<AwardDto>> AllAwardsCache()
		{
			if (_cache.TryGetValue(AwardCacheKey, out IEnumerable<AwardDto> awards))
			{
				return awards;
			}

			using (_db.Database.BeginTransactionAsync())
			{
				var userAwards = await _db.UserAwards
					.Select(ua => new AwardDto
					{
						ShortName = ua.Award.ShortName,
						Year = ua.Year,
						Type = AwardType.User,
						PublicationId = null,
						UserIds = new[] { ua.UserId }
					})
					.ToListAsync();

				var movieAwards = await _db.PublicationAwards
					.Select(pa => new AwardDto
					{
						ShortName = pa.Award.ShortName,
						Year = pa.Year,
						Type = AwardType.Movie,
						PublicationId = pa.PublicationId,
						UserIds = pa.Publication.Authors
							.Select(a => a.UserId)
							.ToList()
					})
					.ToListAsync();

				var allAwards = userAwards.Concat(movieAwards);

				_cache.Set(AwardCacheKey, allAwards, DurationConstants.OneWeekInSeconds);

				return allAwards;
			}
		}
	}
}
