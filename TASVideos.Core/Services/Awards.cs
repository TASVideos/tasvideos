using TASVideos.Data.Entity.Awards;

namespace TASVideos.Core.Services;

public interface IAwards
{
	/// <summary>
	/// Gets all awards for the given user,
	/// or any movie for which the user is an author of
	/// </summary>
	ValueTask<ICollection<AwardAssignmentSummary>> ForUser(int userId);

	/// <summary>
	/// Gets all awards for the given publication
	/// </summary>
	ValueTask<IEnumerable<AwardAssignmentSummary>> ForPublication(int publicationId);

	/// <summary>
	/// Gets all awards assigned in the given year
	/// </summary>
	/// <param name="year">The year, ex: 2010</param>
	ValueTask<ICollection<AwardAssignment>> ForYear(int year);

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

internal class Awards(ApplicationDbContext db, ICacheService cache) : IAwards
{
	public async ValueTask<ICollection<AwardAssignmentSummary>> ForUser(int userId)
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

	public async ValueTask<ICollection<AwardAssignment>> ForYear(int year)
	{
		var allAwards = await AllAwards();
		return allAwards
			.Where(a => a.Year == year)
			.ToList();
	}

	public async Task<bool> AddAwardCategory(AwardType type, string shortName, string description)
	{
		if (await db.Awards.AnyAsync(a => a.ShortName == shortName))
		{
			return false;
		}

		db.Awards.Add(new Award
		{
			Type = type,
			ShortName = shortName,
			Description = description
		});
		await db.SaveChangesAsync();

		return true;
	}

	public Task<bool> CategoryExists(string shortName)
	{
		return db.Awards.AnyAsync(a => EF.Functions.Like(a.ShortName, shortName));
	}

	public IQueryable<Award> AwardCategories() => db.Awards.AsQueryable();

	public async Task AssignUserAward(string shortName, int year, IEnumerable<int> userIds)
	{
		var award = await db.Awards.SingleAsync(a => a.ShortName == shortName);

		var existingUsers = await db.UserAwards
			.Where(ua => ua.Year == year
				&& ua.Award!.ShortName == shortName
				&& userIds.Contains(ua.UserId))
			.Select(pa => pa.UserId)
			.ToListAsync();

		db.UserAwards.AddRange(userIds
			.Except(existingUsers)
			.Select(u => new UserAward
			{
				Year = year,
				UserId = u,
				AwardId = award.Id
			}));
		await db.SaveChangesAsync();
		await FlushCache();
	}

	public async Task AssignPublicationAward(string shortName, int year, IEnumerable<int> publicationIds)
	{
		var pubIds = publicationIds.ToList();
		var award = await db.Awards.SingleAsync(a => a.ShortName == shortName);

		var existingPubs = await db.PublicationAwards
			.Where(pa => pa.Year == year
				&& pa.Award!.ShortName == shortName
				&& pubIds.Contains(pa.PublicationId))
			.Select(pa => pa.PublicationId)
			.ToListAsync();

		db.PublicationAwards.AddRange(pubIds
			.Except(existingPubs)
			.Select(u => new PublicationAward
			{
				Year = year,
				PublicationId = u,
				AwardId = award.Id
			}));

		await db.SaveChangesAsync();
		await FlushCache();
	}

	public async Task Revoke(AwardAssignment award)
	{
		var userAwardsToRemove = await db.UserAwards
			.Where(ua => ua.Award!.Id == award.AwardId && ua.Year == award.Year)
			.ToListAsync();

		db.UserAwards.RemoveRange(userAwardsToRemove);

		var pubAwardsToRemove = await db.PublicationAwards
			.Where(pa => pa.Award!.Id == award.AwardId && pa.Year == award.Year)
			.ToListAsync();

		db.PublicationAwards.RemoveRange(pubAwardsToRemove);

		await db.SaveChangesAsync();
		await FlushCache();
	}

	public async Task FlushCache()
	{
		cache.Remove(CacheKeys.AwardsCache);
		await AllAwards();
	}

	private async ValueTask<IEnumerable<AwardAssignment>> AllAwards()
	{
		if (cache.TryGetValue(CacheKeys.AwardsCache, out IEnumerable<AwardAssignment> awards))
		{
			return awards;
		}

		var userLists = await db.UserAwards
			.Select(ua => new
			{
				AwardId = ua.Award!.Id,
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
					gkey.AwardId,
					gkey.Description,
					gkey.ShortName,
					gkey.Year
				},
				gvalue => new AwardAssignmentUser(gvalue.UserId, gvalue.UserName))
			.Select(g => new AwardAssignment(
				g.Key.AwardId,
				g.Key.ShortName,
				g.Key.Description + " of " + g.Key.Year,
				g.Key.Year,
				AwardType.User,
				[],
				[.. g]))
			.ToList();

		var pubLists = await db.PublicationAwards
			.Select(pa => new
			{
				AwardId = pa.Award!.Id,
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
					gkey.AwardId,
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
			.Select(g => new AwardAssignment(
				g.Key.AwardId,
				g.Key.ShortName,
				g.Key.Description + " of " + g.Key.Year,
				g.Key.Year,
				AwardType.Movie,
				[.. g.Select(gv => new AwardAssignmentPublication(gv.Publication.Id, gv.Publication.Title))],
				[.. g.SelectMany(gv => gv.Users).Select(u => new AwardAssignmentUser(u.UserId, u.UserName))]))
			.ToList();

		var allAwards = userAwards.Concat(publicationAwards).ToList();

		cache.Set(CacheKeys.AwardsCache, allAwards, Durations.OneWeek);

		return allAwards;
	}
}

/// <summary>
/// Represents the assignment of an award to a user or movie
/// Ex: 2010 TASer of the Year.
/// </summary>
public record AwardAssignment(
	int AwardId,
	string ShortName,
	string Description,
	int Year,
	AwardType Type,
	ICollection<AwardAssignmentPublication> Publications,
	ICollection<AwardAssignmentUser> Users) : AwardAssignmentSummary(ShortName, Description, Year);

public record AwardAssignmentUser(int Id, string UserName);
public record AwardAssignmentPublication(int Id, string Title);
public record AwardAssignmentSummary(string ShortName, string Description, int Year);
