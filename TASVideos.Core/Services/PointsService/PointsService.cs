using Microsoft.EntityFrameworkCore;
using TASVideos.Data;
using TASVideos.Data.Entity;

namespace TASVideos.Core.Services;

public interface IPointsService
{
	/// <summary>
	/// Calculates the player points for the user with the given id. In addition,
	/// the calculated player rank is returned. If a user with the given
	/// <see cref="userId"/> does not exist, 0 is returned
	/// </summary>
	ValueTask<(double, string)> PlayerPoints(int userId);

	/// <summary>
	/// Calculates the player points that are being awarded for the given publication
	/// If a publication with the given <see cref="publicationId"/> does not exist, 0 is returned
	/// </summary>
	ValueTask<double> PlayerPointsForPublication(int publicationId);
}

internal class PointsService : IPointsService
{
	private const string MoviePlayerPointKey = "PlayerPointsForPub-";
	private const string PlayerPointKey = "PlayerPoints-";
	private const string AverageNumberOfRatingsKey = "AverageNumberOfRatings";

	private readonly ApplicationDbContext _db;
	private readonly ICacheService _cache;

	public PointsService(
		ApplicationDbContext db,
		ICacheService cache)
	{
		_db = db;
		_cache = cache;
	}

	public async ValueTask<(double, string)> PlayerPoints(int userId)
	{
		string cacheKey = PlayerPointKey + userId;
		if (_cache.TryGetValue(cacheKey, out double playerPoints))
		{
			return (playerPoints, PointsCalculator.PlayerRank((decimal)playerPoints));
		}

		var publications = await _db.Publications
			.ForAuthor(userId)
			.ToCalcPublication()
			.ToListAsync();

		var averageRatings = await AverageNumberOfRatingsPerPublication();
		playerPoints = Math.Round(PointsCalculator.PlayerPoints(publications, averageRatings), 1);

		_cache.Set(cacheKey, playerPoints);
		return (playerPoints, PointsCalculator.PlayerRank((decimal)playerPoints));
	}

	public async ValueTask<double> PlayerPointsForPublication(int publicationId)
	{
		string cacheKey = MoviePlayerPointKey + publicationId;
		if (_cache.TryGetValue(cacheKey, out double playerPoints))
		{
			return playerPoints;
		}

		var publication = await _db.Publications
			.ToCalcPublication()
			.SingleOrDefaultAsync(p => p.Id == publicationId);

		if (publication is null || publication.AuthorCount == 0)
		{
			return 0;
		}

		var averageRatings = await AverageNumberOfRatingsPerPublication();
		playerPoints = PointsCalculator.PlayerPointsForMovie(publication, averageRatings);
		_cache.Set(cacheKey, playerPoints);
		return playerPoints;
	}

	// total ratings / total publications
	private async ValueTask<double> AverageNumberOfRatingsPerPublication()
	{
		if (_cache.TryGetValue(AverageNumberOfRatingsKey, out double playerPoints))
		{
			return playerPoints;
		}

		var totalPublications = await _db.Publications.CountAsync();

		double avg = 0;
		if (totalPublications > 0)
		{
			avg = await _db.PublicationRatings.CountAsync() / (double)totalPublications;
		}

		_cache.Set(AverageNumberOfRatingsKey, avg);
		return avg;
	}
}

internal static class PointsEntityExtensions
{
	public static IQueryable<PointsCalculator.Publication> ToCalcPublication(this IQueryable<Publication> query)
	{
		return query.Select(p => new PointsCalculator.Publication
		{
			Id = p.Id,
			Obsolete = p.ObsoletedById.HasValue,
			ClassWeight = p.PublicationClass!.Weight,
			AuthorCount = p.Authors.Count,
			RatingCount = p.PublicationRatings.Count,
			AverageRating = p.PublicationRatings.Count > 0 ? p.PublicationRatings
				.Where(pr => !pr.Publication!.Authors.Select(a => a.UserId).Contains(pr.UserId))
				.Where(pr => pr.User!.UseRatings)
				.Average(pr => pr.Value) : null
		});
	}
}