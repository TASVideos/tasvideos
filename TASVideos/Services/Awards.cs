using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using Microsoft.EntityFrameworkCore;

using TASVideos.Data;
using TASVideos.Data.Entity.Awards;

namespace TASVideos.Services
{
	public interface IAwards
	{
		/// <summary>
		/// Gets all awards for the given user,
		/// or any movie for which the user is an author of
		/// </summary>
		Task<IEnumerable<AwardAssignmentSummary>> ForUser(int userId);

		/// <summary>
		/// Gets all awards assigned in the given year
		/// </summary>
		/// <param name="year">The year, ex: 2010</param>
		Task<IEnumerable<AwardAssignment>> ForYear(int year);

		/// <summary>
		/// Clears the awards cache
		/// </summary>
		Task FlushCache();
	}

	public class Awards : IAwards
	{
		private readonly ApplicationDbContext _db;
		private readonly ICacheService _cache;

		public Awards(
			ApplicationDbContext db,
			ICacheService cache)
		{
			_db = db;
			_cache = cache;
		}

		public async Task<IEnumerable<AwardAssignmentSummary>> ForUser(int userId)
		{
			var allAwards = await AllAwards();

			return allAwards
				.Where(a => a.Users.Select(u => u.Id).Contains(userId))
				.Select(ua => new AwardAssignmentSummary
				{
					ShortName = ua.ShortName,
					Description = ua.Description,
					Year = ua.Year
				})
				.ToList();
		}

		public async Task<IEnumerable<AwardAssignment>> ForYear(int year)
		{
			var allAwards = await AllAwards();
			return allAwards
				.Where(a => a.Year + 2000 == year)
				.ToList();
		}

		public async Task FlushCache()
		{
			_cache.Remove(CacheKeys.AwardsCache);
			await AllAwards();
		}

		private async Task<IEnumerable<AwardAssignment>> AllAwards()
		{
			if (_cache.TryGetValue(CacheKeys.AwardsCache, out IEnumerable<AwardAssignment> awards))
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
						gvalue => new AwardAssignment.UserDto
						{
							Id = gvalue.UserId, UserName = gvalue.User.UserName
						})
					.Select(g => new AwardAssignment
					{
						ShortName = g.Key.ShortName,
						Description = g.Key.Description + " of " + (g.Key.Year + 2000).ToString(),
						Year = g.Key.Year,
						Type = AwardType.User,
						Publications = Enumerable.Empty<AwardAssignment.PublicationDto>(),
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
					.Select(g => new AwardAssignment
					{
						ShortName = g.Key.ShortName,
						Description = g.Key.Description + " of " + (g.Key.Year + 2000).ToString(),
						Year = g.Key.Year,
						Type = AwardType.Movie,
						Publications = g.Select(gv => new AwardAssignment.PublicationDto { Id = gv.Publication.Id, Title  = gv.Publication.Title }).ToList(),
						Users = g.SelectMany(gv => gv.Users).Select(u => new AwardAssignment.UserDto { Id = u.UserId, UserName = u.UserName }).ToList()
					})
					.ToList();

				var allAwards = userAwards.Concat(publicationAwards);

				_cache.Set(CacheKeys.AwardsCache, allAwards, Durations.OneWeekInSeconds);

				return allAwards;
			}
		}
	}
}
