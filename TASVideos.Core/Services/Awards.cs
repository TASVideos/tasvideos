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
	/// Adds an award for the given year
	/// </summary>
	/// <returns>False if the award cannot be added.</returns>
	Task<bool> AddAwardCategory(AwardType type, string shortName, string description);

	Task<bool> CategoryExists(string shortName);

	IQueryable<Award> AwardCategories();

	Task AssignUserAward(string shortName, int year, IEnumerable<int> userIds);
	Task AssignPublicationAward(string shortName, int year, IEnumerable<int> publicationIds);

	Task Revoke(AwardAssignment award);

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
				.Select(_ => new AwardAssignmentSummary(a.ShortName, a.Description, a.Year)))
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

	public async Task<bool> AddAwardCategory(AwardType type, string shortName, string description)
	{
		if (await _db.Awards.AnyAsync(a => a.ShortName == shortName))
		{
			return false;
		}

		_db.Awards.Add(new Award
		{
			Type = type,
			ShortName = shortName,
			Description = description
		});
		await _db.SaveChangesAsync();

		return true;
	}

	public Task<bool> CategoryExists(string shortName)
	{
		return _db.Awards.AnyAsync(a => EF.Functions.Like(a.ShortName, shortName));
	}

	public IQueryable<Award> AwardCategories() => _db.Awards.AsQueryable();

	public async Task AssignUserAward(string shortName, int year, IEnumerable<int> userIds)
	{
		var award = await _db.Awards.SingleAsync(a => a.ShortName == shortName);

		var existingUsers = await _db.UserAwards
			.Where(ua => ua.Year == year
				&& ua.Award!.ShortName == shortName
				&& userIds.Contains(ua.UserId))
			.Select(pa => pa.UserId)
			.ToListAsync();

		_db.UserAwards.AddRange(userIds
			.Except(existingUsers)
			.Select(u => new UserAward
			{
				Year = year,
				UserId = u,
				AwardId = award.Id
			}));
		await _db.SaveChangesAsync();
		await FlushCache();
	}

	public async Task AssignPublicationAward(string shortName, int year, IEnumerable<int> publicationIds)
	{
		var pubIds = publicationIds.ToList();
		var award = await _db.Awards.SingleAsync(a => a.ShortName == shortName);

		var existingPubs = await _db.PublicationAwards
			.Where(pa => pa.Year == year
				&& pa.Award!.ShortName == shortName
				&& pubIds.Contains(pa.PublicationId))
			.Select(pa => pa.PublicationId)
			.ToListAsync();

		_db.PublicationAwards.AddRange(pubIds
			.Except(existingPubs)
			.Select(u => new PublicationAward
			{
				Year = year,
				PublicationId = u,
				AwardId = award.Id
			}));

		await _db.SaveChangesAsync();
		await FlushCache();
	}

	public async Task Revoke(AwardAssignment award)
	{
		var userIdsToRemove = award.Users.Select(u => u.Id);
		var userAwardsToRemove = await _db.UserAwards
			.Where(ua => userIdsToRemove.Contains(ua.UserId))
			.ToListAsync();

		_db.UserAwards.RemoveRange(userAwardsToRemove);

		var pubIdsToRemove = award.Publications.Select(p => p.Id);
		var pubAwardsToRemove = await _db.PublicationAwards
			.Where(pa => pubIdsToRemove.Contains(pa.PublicationId))
			.ToListAsync();

		_db.PublicationAwards.RemoveRange(pubAwardsToRemove);

		await _db.SaveChangesAsync();
		await FlushCache();
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
				Publications = Array.Empty<AwardAssignment.Publication>(),
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
