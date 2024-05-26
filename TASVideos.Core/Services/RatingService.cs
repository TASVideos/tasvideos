namespace TASVideos.Core.Services;

public interface IRatingService
{
	Task<PublicationRating?> GetUserRatingForPublication(int userId, int publicationId);
	Task<ICollection<RatingEntry>> GetRatingsForPublication(int publicationId);
	Task<double> GetOverallRatingForPublication(int publicationId);
	Task<SaveResult> UpdateUserRating(int userId, int publicationId, double? value);
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
		return await db.Publications
			.Where(p => p.Id == publicationId)
			.Select(p => p.PublicationRatings
			.Where(pr => !pr.Publication!.Authors.Select(a => a.UserId).Contains(pr.UserId))
			.Where(pr => pr.User!.UseRatings)
			.Average(pr => pr.Value))
			.SingleOrDefaultAsync();
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
}

public record RatingEntry(string UserName, double Rating, bool IsPublic);
