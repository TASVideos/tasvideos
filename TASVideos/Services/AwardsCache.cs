using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using Microsoft.EntityFrameworkCore;

using TASVideos.Data;
using TASVideos.Data.Constants;
using TASVideos.Data.Entity.Awards;

namespace TASVideos.Services
{
	// TODO: can this be turned into a collection somehow?
	public interface IAwardsCache
	{
		Task<IEnumerable<AwardDto>> Awards();
		Task<IEnumerable<AwardEntryDto>> AwardsForUser(int userId);
		Task Flush();
	}

	public class AwardsCache : IAwardsCache
	{
		private readonly ApplicationDbContext _db;
		private readonly ICacheService _cache;

		public AwardsCache(
			ApplicationDbContext db,
			ICacheService cache)
		{
			_db = db;
			_cache = cache;
		}

		public async Task Flush()
		{
			_cache.Remove(CacheKeys.AwardsCache);
			await Awards();
		}

		public async Task<IEnumerable<AwardDto>> Awards()
		{
			if (_cache.TryGetValue(CacheKeys.AwardsCache, out IEnumerable<AwardDto> awards))
			{
				return awards;
			}

			// TODO: optimize these with EF 2.1, 2.0 is so bad with GroupBy that it is hopeless
			using (await _db.Database.BeginTransactionAsync())
			{
				var userAwards = await _db.UserAwards
					.GroupBy(
						gkey => new
						{
							gkey.Award.Description, gkey.Award.ShortName, gkey.Year
						}, 
						gvalue => new AwardDto.UserDto
						{
							Id = gvalue.UserId, UserName = gvalue.User.UserName
						})
					.Select(g => new AwardDto
					{
						ShortName = g.Key.ShortName,
						Description = g.Key.Description + " of " + (g.Key.Year + 2000).ToString(),
						Year = g.Key.Year,
						Type = AwardType.User,
						Publications = Enumerable.Empty<AwardDto.PublicationDto>(),
						Users = g.ToList()
					})
					.ToListAsync();

				var pubLists = await _db.PublicationAwards
					.Include(pa => pa.Award)
					.Include(pa => pa.Publication)
					.ThenInclude(pa => pa.Authors)
					.ThenInclude(a => a.Author)
					.ToListAsync();

				var publicationAwards = pubLists
					.GroupBy(
						gkey => new
						{
							gkey.Award.Description, gkey.Award.ShortName, gkey.Year
						},
						gvalue => new
						{
							Publication = new
							{
								Id = gvalue.PublicationId,
								gvalue.Publication.Title
							},
							Users = gvalue.Publication.Authors.Select(a => new
							{
								a.UserId, a.Author.UserName
							})
						})
					.Select(g => new AwardDto
					{
						ShortName = g.Key.ShortName,
						Description = g.Key.Description + " of " + (g.Key.Year + 2000).ToString(),
						Year = g.Key.Year,
						Type = AwardType.Movie,
						Publications = g.Select(gv => new AwardDto.PublicationDto { Id = gv.Publication.Id, Title  = gv.Publication.Title }).ToList(),
						Users = g.SelectMany(gv => gv.Users).Select(u => new AwardDto.UserDto { Id = u.UserId, UserName = u.UserName }).ToList()
					})
					.ToList();

				var allAwards = userAwards.Concat(publicationAwards);

				_cache.Set(CacheKeys.AwardsCache, allAwards, Durations.OneWeekInSeconds);

				return allAwards;
			}
		}

		/// <summary>
		/// Gets all awards for the user, or any movie for which the user is an author of
		/// </summary>
		public async Task<IEnumerable<AwardEntryDto>> AwardsForUser(int userId)
		{
			var allAwards = await Awards();

			return allAwards
				.Where(a => a.Users.Select(u => u.Id).Contains(userId))
				.Select(ua => new AwardEntryDto
				{
					ShortName = ua.ShortName,
					Description = ua.Description,
					Year = ua.Year
				})
				.ToList();
		}
	}
}
