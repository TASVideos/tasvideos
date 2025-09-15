namespace TASVideos.Core.Services;

public interface IRatingService
{
	Task<PublicationRating?> GetUserRatingForPublication(int userId, int publicationId);
	Task<ICollection<RatingEntry>> GetRatingsForPublication(int publicationId);
	Task<double> GetOverallRatingForPublication(int publicationId);
	Task<SaveResult> UpdateUserRating(int userId, int publicationId, double? value);

	/// <summary>
	/// Returns the rating information for the given user
	/// If user is not found, null is returned
	/// If user has PublicRatings false, then the ratings will be an empty list
	/// </summary>
	Task<UserRatings?> GetUserRatings(string userName, RatingRequest paging, bool includeHidden = false);
}

internal class RatingService(ApplicationDbContext db) : IRatingService
{
	public async Task<PublicationRating?> GetUserRatingForPublication(int userId, int publicationId)
	{
		return await db.PublicationRatings
			.ForPublication(publicationId)
			.ForUser(userId)
			.FirstOrDefaultAsync();
	}

	public async Task<ICollection<RatingEntry>> GetRatingsForPublication(int publicationId)
	{
		return await db.PublicationRatings
			.ForPublication(publicationId)
			.Select(pr => new RatingEntry(pr.User!.UserName, pr.Value, pr.User!.PublicRatings))
			.ToListAsync();
	}

	public async Task<double> GetOverallRatingForPublication(int publicationId)
	{
		var ratings = await db.PublicationRatings
			.Where(pr => pr.PublicationId == publicationId)
			.Where(pr => !pr.Publication!.Authors.Select(a => a.UserId).Contains(pr.UserId))
			.Where(pr => pr.User!.UseRatings)
			.Select(pr => pr.Value)
			.ToListAsync();

		if (ratings.Count == 0)
		{
			return 0;
		}

		return ratings.Average();
	}

	public async Task<SaveResult> UpdateUserRating(int userId, int publicationId, double? value)
	{
		var rating = await GetUserRatingForPublication(userId, publicationId);
		if (rating is not null)
		{
			if (value.HasValue)
			{
				rating.Value = value.Value;
			}
			else
			{
				db.PublicationRatings.Remove(rating);
			}
		}
		else if (value.HasValue)
		{
			db.PublicationRatings.Add(new PublicationRating
			{
				PublicationId = publicationId,
				UserId = userId,
				Value = Math.Round(value.Value, 1)
			});
		}
		else
		{
			return SaveResult.Success;
		}

		return await db.TrySaveChanges();
	}

	public async Task<UserRatings?> GetUserRatings(string userName, RatingRequest paging, bool includeHidden = false)
	{
		var dto = await db.Users
			.ForUser(userName)
			.Select(u => new UserRatings
			{
				Id = u.Id,
				UserName = u.UserName,
				PublicRatings = u.PublicRatings
			})
			.SingleOrDefaultAsync();

		if (dto is null)
		{
			return null;
		}

		dto.RatingsCount = await db.PublicationRatings
			.ForUser(dto.Id)
			.IncludeObsolete(false)
			.CountAsync();

		dto.Summary.TotalPublicationCount = await db.Publications
			.ThatAreCurrent()
			.CountAsync();

		var allTopRaters = await db.PublicationRatings
			.IncludeObsolete(false)
			.GroupBy(pr => pr.User)
			.Select(group => new
			{
				group.Key!.UserName,
				RatingsCount = group.Count()
			})
			.OrderByDescending(tr => tr.RatingsCount)
			.ToListAsync();

		dto.Summary.TopRaters = allTopRaters
			.GroupBy(tr => tr.RatingsCount)
			.Select(group => new UserRatings.RatingSummary.TopRaterEntry
			{
				RatingsCount = group.Key,
				UserNames = group.Select(tr => tr.UserName).OrderBy(userName => userName).ToList()
			})
			.OrderByDescending(group => group.RatingsCount)
			.Take(30)
			.ToList();

		dto.Summary.TotalRaterCount = allTopRaters.Count;
		dto.UsersWithLowerRatingsCount = allTopRaters.Count(tr => tr.RatingsCount < dto.RatingsCount);
		dto.UsersWithEqualRatingsCount = dto.RatingsCount == 0 ? 0 : allTopRaters.Count(tr => tr.RatingsCount == dto.RatingsCount) - 1; // subtract the user themself
		dto.UsersWithHigherRatingsCount = allTopRaters.Count(tr => tr.RatingsCount > dto.RatingsCount);

		if (!dto.PublicRatings && !includeHidden)
		{
			return dto;
		}

		dto.Ratings = await db.PublicationRatings
			.ForUser(dto.Id)
			.IncludeObsolete(paging.IncludeObsolete)
			.Select(pr => new UserRatings.Rating
			{
				PublicationId = pr.PublicationId,
				PublicationTitle = pr.Publication!.Title,
				IsObsolete = pr.Publication.ObsoletedById.HasValue,
				Value = pr.Value
			})
			.SortedPageOf(paging);

		return dto;
	}
}

public record RatingEntry(string UserName, double Rating, bool IsPublic);

public class UserRatings
{
	public int Id { get; init; }
	public string UserName { get; init; } = "";
	public bool PublicRatings { get; init; }

	public PageOf<Rating, RatingRequest> Ratings { get; set; } = new([], new());

	public RatingSummary Summary { get; set; } = new();
	public int RatingsCount { get; set; }
	public int UsersWithLowerRatingsCount { get; set; }
	public int UsersWithEqualRatingsCount { get; set; }
	public int UsersWithHigherRatingsCount { get; set; }

	public class Rating
	{
		[TableIgnore]
		public int PublicationId { get; init; }

		[Sortable]
		public string PublicationTitle { get; init; } = "";

		[Sortable]
		public double Value { get; init; }

		[Sortable]
		public bool IsObsolete { get; init; }
	}

	public class RatingSummary
	{
		public List<TopRaterEntry> TopRaters { get; set; } = [];
		public int TotalRaterCount { get; set; }
		public int TotalPublicationCount { get; set; }

		public class TopRaterEntry
		{
			public int RatingsCount { get; set; }
			public List<string> UserNames { get; set; } = [];
		}
	}
}

[PagingDefaults(PageSize = 50, Sort = "-Value")]
public class RatingRequest : PagingModel
{
	public bool IncludeObsolete { get; set; }
}
