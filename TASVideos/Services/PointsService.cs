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
		/// Returns the player point calculation for the user with the given id
		/// </summary>
		/// <exception>If a user with the given id does not exist</exception>
		Task<int> CalculatePointsForUser(int id);

		/// <summary>
		/// Returns the averaged overall, tech, and entertainment ratings for a publication
		/// with the given id
		/// </summary>
		/// <exception>If a publication with the given id does not exist</exception>
		Task<RatingDto> CalculatePublicationRatings(int id);

		/// <summary>
		/// A bulk version of <see cref="CalculatePublicationRatings(int)"/>
		/// </summary>
		Task<IDictionary<int, RatingDto>> CalculatePublicationRatings(IEnumerable<int> publicationIds);
	}

	public class PointsService : IPointsService
    {
		private const string MovieRatingKey = "OverallRatingForMovie-";
		private const string PlayerPointKey = "PlayerPoints-";

		private readonly ApplicationDbContext _db;
		private readonly ICacheService _cache;

		public PointsService(
			ApplicationDbContext db,
			ICacheService cache)
		{
			_db = db;
			_cache = cache;
		}

		public async Task<int> CalculatePointsForUser(int id)
		{
			string cacheKey = PlayerPointKey + id;
			if (_cache.TryGetValue(cacheKey, out int playerPoints))
			{
				return playerPoints;
			}

			var user = await _db.Users.SingleAsync(u => u.Id == id);

			var publications = await _db.Publications
				.Where(p => p.Authors.Select(pa => pa.UserId).Contains(user.Id))
				.ToListAsync();

			playerPoints = new Random(DateTime.Now.Millisecond).Next(0, 10000);
			_cache.Set(cacheKey, playerPoints);

			return playerPoints;
		}
		
		public async Task<RatingDto> CalculatePublicationRatings(int id)
		{
			string cacheKey = MovieRatingKey + id;
			if (_cache.TryGetValue(cacheKey, out RatingDto rating))
			{
				return rating;
			}

			var ratings = await _db.PublicationRatings
				.Where(pr => pr.PublicationId == id)
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

		public async Task<IDictionary<int, RatingDto>> CalculatePublicationRatings(IEnumerable<int> publicationIds)
		{
			if (publicationIds == null)
			{
				throw new ArgumentException($"{nameof(publicationIds)} can not be null");
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
						rating = await CalculatePublicationRatings(id);
						_cache.Set(cacheKey, rating);
						ratings.Add(id, rating);
					}
				}
			}

			return ratings;
		}
	}
}
