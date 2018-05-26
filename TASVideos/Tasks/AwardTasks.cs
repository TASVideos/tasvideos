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

		public async Task<AwardByYearModel> GetAwardsForModule(int year)
		{
			var allAwards = await AllAwardsCache();

			var model = allAwards
				.Where(a => a.Year + 2000 == year)
				.ToList();


			return new AwardByYearModel();
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

		private async Task<IEnumerable<AwardDto>> AllAwardsCache()
		{
			if (_cache.TryGetValue(AwardCacheKey, out IEnumerable<AwardDto> awards))
			{
				return awards;
			}

			// TODO: optimize these with EF 2.1, 2.0 is so bad with GroupBy that it is hopeless
			using (_db.Database.BeginTransactionAsync())
			{
				var userAwards = await _db.UserAwards
					.GroupBy(gkey => new { gkey.Award.Description, gkey.Award.ShortName, gkey.Year }, gvalue => gvalue.UserId)
					.Select(g => new AwardDto
					{
						ShortName = g.Key.ShortName,
						Description = g.Key.Description + " of " + (g.Key.Year + 2000).ToString(),
						Year = g.Key.Year,
						Type = AwardType.User,
						PublicationIds = Enumerable.Empty<int>(),
						UserIds = g.Select(userId => userId).ToList()
					})
					.ToListAsync();


				var pubLists = await _db.PublicationAwards
					.Include(pa => pa.Award)
					.Include(pa => pa.Publication)
					.ThenInclude(pa => pa.Authors)
					.ToListAsync();

				var publicationAwards = pubLists
					.GroupBy(gkey => new { gkey.Award.Description, gkey.Award.ShortName, gkey.Year }, gvalue => new { gvalue.PublicationId, UserIds = gvalue.Publication.Authors.Select(a => a.UserId) })
					.Select(g => new AwardDto
					{
						ShortName = g.Key.ShortName,
						Description = g.Key.Description + " of " + (g.Key.Year + 2000).ToString(),
						Year = g.Key.Year,
						Type = AwardType.User,
						PublicationIds = g.Select(gv => gv.PublicationId).ToList(),
						UserIds = g.SelectMany(gv => gv.UserIds).ToList()
					})
					.ToList();

				var allAwards = userAwards.Concat(publicationAwards);

				_cache.Set(AwardCacheKey, allAwards, DurationConstants.OneWeekInSeconds);

				return allAwards;
			}
		}

		private class AwardDto
		{
			public string ShortName { get; set; }
			public string Description { get; set; }
			public int Year { get; set; }
			public AwardType Type { get; set; }
			public IEnumerable<int> PublicationIds { get; set; } = new HashSet<int>();
			public IEnumerable<int> UserIds { get; set; } = new HashSet<int>();
		}
	}
}
