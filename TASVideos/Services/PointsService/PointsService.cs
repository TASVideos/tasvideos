using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

using TASVideos.Data;
using TASVideos.Data.Entity;
using TASVideos.Services.Dtos;

namespace TASVideos.Services
{
	public interface IPointsService
	{
		/// <summary>
		/// Calculates the player points for the user with the given id.
		/// If a user with the given <see cref="userId"/> does not exist, 0 is returned
		/// </summary>
		Task<double> PlayerPoints(int userId);

		/// <summary>
		/// Returns the averaged overall, tech, and entertainment ratings for a publication
		/// with the given id.
		/// </summary>
		Task<RatingDto> PublicationRating(int id);

		/// <summary>
		/// Returns the averaged overall, tech, and entertainment ratings for a publication
		/// with the given set of ids
		/// <seealso cref="PublicationRating"/>
		/// </summary>
		Task<IDictionary<int, RatingDto>> PublicationRatings(IEnumerable<int> publicationIds);
	}

	public class PointsService : IPointsService
	{
		private const string MovieRatingKey = "OverallRatingForMovie-";
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

		public async Task<double> PlayerPoints(int userId)
		{
			string cacheKey = PlayerPointKey + userId;
			if (_cache.TryGetValue(cacheKey, out double playerPoints))
			{
				return playerPoints;
			}

			var user = await _db.Users
				.SingleOrDefaultAsync(u => u.Id == userId);
			if (user == null)
			{
				return 0;
			}

			var dtos = await _db.Publications
				.Where(p => p.Authors.Select(pa => pa.UserId).Contains(user.Id))
				.Select(p => new PointsCalculator.Publication
				{
					Id = p.Id,
					AuthorCount = p.Authors.Count,
					Obsolete = p.ObsoletedById.HasValue,
					TierWeight = p.Tier.Weight,
					RatingCount = p.PublicationRatings.Count,
				})
				.ToListAsync();

			var publications = dtos
				.Select(p => new PointsCalculator.Publication
				{
					Id = p.Id,
					AuthorCount = p.AuthorCount,
					Obsolete = p.Obsolete,
					TierWeight = p.TierWeight,
					RatingCount = p.RatingCount,
				})
				.ToList();

			foreach (var pub in publications)
			{
				pub.AverageRating = (await PublicationRating(pub.Id)).Overall ?? 0;
			}

			var averageRatings = await AverageNumberOfRatingsPerPublication();

			playerPoints = Math.Round(PointsCalculator.PlayerPoints(publications, averageRatings), 1);

			_cache.Set(cacheKey, playerPoints);
			return playerPoints;
		}

		// TODO: user weights
		// TODO: pass in authorId optionally, to exclude them
		public async Task<RatingDto> PublicationRating(int id)
		{
			string cacheKey = MovieRatingKey + id;
			if (_cache.TryGetValue(cacheKey, out RatingDto rating))
			{
				return rating;
			}

			// TODO: do this in a non-hacky way, this logic was directly lifted from legacy system
			// Specifically banned from rating.
			var banned = new[] { 7194, 4805, 4485, 5243, 635, 3301 };

			var ratings = await _db.PublicationRatings
				.Where(pr => pr.PublicationId == id)
				.Where(pr => !banned.Contains(pr.UserId))
				.ToListAsync();

			var entRatings = ratings
				.Where(r => r.Type == PublicationRatingType.Entertainment)
				.Select(r => r.Value)
				.ToList();

			var techRatings = ratings
				.Where(r => r.Type == PublicationRatingType.TechQuality)
				.Select(r => r.Value)
				.ToList();

			rating = new RatingDto
			{
				Entertainment = entRatings.Any()
					? entRatings.Average()
					: (double?)null,
				TechQuality = techRatings.Any()
					? techRatings.Average()
					: (double?)null,
				TotalEntertainmentVotes = entRatings.Count,
				TotalTechQualityVotes = techRatings.Count
			};

			if (entRatings.Any() || techRatings.Any())
			{
				// Entertainment counts 2:1 over Tech
				rating.Overall = entRatings
					.Concat(entRatings)
					.Concat(techRatings)
					.Average();
			}

			_cache.Set(cacheKey, rating);
			return rating;
		}

		public async Task<IDictionary<int, RatingDto>> PublicationRatings(IEnumerable<int> publicationIds)
		{
			if (publicationIds == null)
			{
				publicationIds = new int[0];
			}

			var ratings = new Dictionary<int, RatingDto>();

			// TODO: select them all at once then calculate
			using (await _db.Database.BeginTransactionAsync())
			{
				foreach (var id in publicationIds)
				{
					var cacheKey = MovieRatingKey + id;
					if (_cache.TryGetValue(cacheKey, out RatingDto rating))
					{
						ratings.Add(id, rating);
					}
					else
					{
						rating = await PublicationRating(id);
						_cache.Set(cacheKey, rating);
						ratings.Add(id, rating);
					}
				}
			}

			return ratings;
		}

		// total ratings / (2 * total publications)
		internal async Task<double> AverageNumberOfRatingsPerPublication()
		{
			if (_cache.TryGetValue(AverageNumberOfRatingsKey, out int playerPoints))
			{
				return playerPoints;
			}

			var totalPublications = await _db.Publications.CountAsync();

			double avg = 0;
			if (totalPublications > 0)
			{
				var totalRatings = await _db.PublicationRatings.CountAsync();
				avg = totalRatings / (double)(2 * totalPublications);
			}
			
			_cache.Set(AverageNumberOfRatingsKey, avg);
			return avg;
		}
	}
}
