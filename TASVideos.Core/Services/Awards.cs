using Microsoft.EntityFrameworkCore;
using TASVideos.Data;
using TASVideos.Data.Entity.Awards;

namespace TASVideos.Core.Services;

public interface IAwards
{
	/// <summary>
	/// Gets all awards for the given user,
	/// or any movie for which the user is an author of
	/// </summary>
	ValueTask<IEnumerable<AwardAssignmentSummary>> ForUser(int userId);

	/// <summary>
	/// Gets all awards for the given publication
	/// </summary>
	ValueTask<IEnumerable<AwardAssignmentSummary>> ForPublication(int publicationId);

	/// <summary>
	/// Gets all awards assigned in the given year
	/// </summary>
	/// <param name="year">The year, ex: 2010</param>
	ValueTask<IEnumerable<AwardAssignment>> ForYear(int year);

	/// <summary>
	/// Clears the awards cache
	/// </summary>
	Task FlushCache();
}

internal class Awards : IAwards
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

	public async ValueTask<IEnumerable<AwardAssignmentSummary>> ForUser(int userId)
	{
		var allAwards = await AllAwards();

		return allAwards
			.Where(a => a.Users.Select(u => u.Id).Contains(userId))
			.SelectMany(a => a.Users
				.Where(u => u.Id == userId)
				.Select(u => new AwardAssignmentSummary(a.ShortName, a.Description, a.Year)))
			.ToList();
	}

	public async ValueTask<IEnumerable<AwardAssignmentSummary>> ForPublication(int publicationId)
	{
		var allAwards = await AllAwards();

		return allAwards
			.Where(a => a.Publications.Select(p => p.Id).Contains(publicationId))
			.Select(pa => new AwardAssignmentSummary(pa.ShortName, pa.Description, pa.Year))
			.ToList();
	}

	public async ValueTask<IEnumerable<AwardAssignment>> ForYear(int year)
	{
		var allAwards = await AllAwards();
		return allAwards
			.Where(a => a.Year == year)
			.ToList();
	}

	public async Task FlushCache()
	{
		_cache.Remove(CacheKeys.AwardsCache);
		await AllAwards();
	}

	private async ValueTask<IEnumerable<AwardAssignment>> AllAwards()
	{
		if (_cache.TryGetValue(CacheKeys.AwardsCache, out IEnumerable<AwardAssignment> awards))
		{
			return awards;
		}

		var userLists = await _db.UserAwards
			.Select(ua => new
			{
				ua.Award!.Description,
				ua.Award.ShortName,
				ua.Year,
				UserId = ua.User!.Id,
				ua.User.UserName
			})
			.ToListAsync();

		var userAwards = userLists
			.GroupBy(
				gkey => new
				{
					gkey.Description,
					gkey.ShortName,
					gkey.Year
				},
				gvalue => new AwardAssignment.User(gvalue.UserId, gvalue.UserName)
				{
					Id = gvalue.UserId,
					UserName = gvalue.UserName
				})
			.Select(g => new AwardAssignment
			{
				ShortName = g.Key.ShortName,
				Description = g.Key.Description + " of " + g.Key.Year,
				Year = g.Key.Year,
				Type = AwardType.User,
				Publications = Enumerable.Empty<AwardAssignment.Publication>(),
				Users = g.ToList()
			})
			.ToList();

		var pubLists = await _db.PublicationAwards
			.Select(pa => new
			{
				pa.Award!.Description,
				pa.Award.ShortName,
				pa.Year,
				pa.PublicationId,
				pa.Publication!.Title,
				Authors = pa.Publication.Authors.OrderBy(paa => paa.Ordinal).Select(a => new { a.UserId, a.Author!.UserName })
			})
			.ToListAsync();

		var publicationAwards = pubLists
			.GroupBy(
				gkey => new
				{
					gkey.Description,
					gkey.ShortName,
					gkey.Year
				},
				gvalue => new
				{
					Publication = new
					{
						Id = gvalue.PublicationId,
						gvalue.Title
					},
					Users = gvalue.Authors.Select(a => new
					{
						a.UserId,
						a.UserName
					})
				})
			.Select(g => new AwardAssignment
			{
				ShortName = g.Key.ShortName,
				Description = g.Key.Description + " of " + g.Key.Year,
				Year = g.Key.Year,
				Type = AwardType.Movie,
				Publications = g
					.Select(gv => new AwardAssignment.Publication(gv.Publication.Id, gv.Publication.Title))
					.ToList(),
				Users = g
					.SelectMany(gv => gv.Users)
					.Select(u => new AwardAssignment.User(u.UserId, u.UserName))
					.ToList()
			})
			.ToList();

		var allAwards = userAwards.Concat(publicationAwards);

		_cache.Set(CacheKeys.AwardsCache, allAwards, Durations.OneWeekInSeconds);

		return allAwards;
	}
}
