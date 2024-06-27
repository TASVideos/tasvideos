namespace TASVideos.Core.Services;

public interface IPointsService
{
	/// <summary>
	/// Calculates the player points for the user with the given id. In addition,
	/// the calculated player rank is returned. If a user with the given
	/// <see cref="userId"/> does not exist, 0 is returned
	/// </summary>
	ValueTask<(double Points, string Rank)> PlayerPoints(int userId);

	/// <summary>
	/// Calculates the player points that are being awarded for the given publication
	/// If a publication with the given <see cref="publicationId"/> does not exist, 0 is returned
	/// </summary>
	ValueTask<double> PlayerPointsForPublication(int publicationId);
}

internal class PointsService(ApplicationDbContext db, ICacheService cache) : IPointsService
{
	private const string MoviePlayerPointKey = "PlayerPointsForPub-";
	private const string PlayerPointKey = "PlayerPoints-";
	private const string AverageNumberOfRatingsKey = "AverageNumberOfRatings";

	public async ValueTask<(double Points, string Rank)> PlayerPoints(int userId)
	{
		string cacheKey = PlayerPointKey + userId;
		if (cache.TryGetValue(cacheKey, out double playerPoints))
		{
			return (playerPoints, PointsCalculator.PlayerRank((decimal)playerPoints));
		}

		var publications = await db.Publications
			.ForAuthor(userId)
			.ToCalcPublication()
			.ToListAsync();

		var averageRatings = await AverageNumberOfRatingsPerPublication();
		playerPoints = Math.Ceiling(PointsCalculator.PlayerPoints(publications, averageRatings) * 10) / 10;

		cache.Set(cacheKey, playerPoints);
		return (playerPoints, PointsCalculator.PlayerRank((decimal)playerPoints));
	}

	public async ValueTask<double> PlayerPointsForPublication(int publicationId)
	{
		string cacheKey = MoviePlayerPointKey + publicationId;
		if (cache.TryGetValue(cacheKey, out double playerPoints))
		{
			return playerPoints;
		}

		var publication = await db.Publications
			.Where(p => p.Id == publicationId)
			.ToCalcPublication()
			.SingleOrDefaultAsync();

		if (publication is null || publication.AuthorCount == 0)
		{
			return 0;
		}

		var averageRatings = await AverageNumberOfRatingsPerPublication();
		playerPoints = PointsCalculator.PlayerPointsForMovie(publication, averageRatings);
		cache.Set(cacheKey, playerPoints);
		return playerPoints;
	}

	// total ratings / total publications
	private async ValueTask<double> AverageNumberOfRatingsPerPublication()
	{
		if (cache.TryGetValue(AverageNumberOfRatingsKey, out double playerPoints))
		{
			return playerPoints;
		}

		var totalPublications = await db.Publications.CountAsync();

		double avg = 0;
		if (totalPublications > 0)
		{
			avg = await db.PublicationRatings.CountAsync() / (double)totalPublications;
		}

		cache.Set(AverageNumberOfRatingsKey, avg);
		return avg;
	}
}

internal static class PointsEntityExtensions
{
	public static IQueryable<PointsCalculator.Publication> ToCalcPublication(this IQueryable<Publication> query)
	{
		return query.Select(p => new PointsCalculator.Publication(
			p.ObsoletedById.HasValue,
			p.PublicationRatings.Count,
			p.Authors.Count,
			p.PublicationFlags.Any() ? p.PublicationFlags.Max(pf => pf.Flag!.Weight) : 1,
			p.PublicationRatings.Count > 0 ? p.PublicationRatings
				.Where(pr => !pr.Publication!.Authors.Select(a => a.UserId).Contains(pr.UserId))
				.Where(pr => pr.User!.UseRatings)
				.Average(pr => pr.Value) : null));
	}
}
