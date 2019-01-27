using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

using TASVideos.Data;
using TASVideos.Data.Entity;
using TASVideos.Models;
using TASVideos.Services;

namespace TASVideos.Tasks
{
	public class RatingsTasks
	{
		private const string MovieRatingKey = "OverallRatingForMovieViewModel-";

		private readonly ApplicationDbContext _db;
		private readonly IPointsService _pointsService;
		private readonly ICacheService _cache;

		public RatingsTasks(
			ApplicationDbContext db,
			IPointsService pointsService,
			ICacheService cache)
		{
			_db = db;
			_pointsService = pointsService;
			_cache = cache;
		}

		// TODO: refactor to use pointsService for calculations
		/// <summary>
		/// Returns a detailed list of all ratings for a <see cref="Publication"/>
		/// with the given <see cref="publicationId"/>
		/// If no <see cref="Publication"/> is found, then null is returned
		/// </summary>
		public async Task<PublicationRatingsModel> GetRatingsForPublication(int publicationId)
		{
			string cacheKey = MovieRatingKey + publicationId;
			if (_cache.TryGetValue(cacheKey, out PublicationRatingsModel rating))
			{
				return rating;
			}

			var publication = await _db.Publications
				.Include(p => p.PublicationRatings)
				.ThenInclude(r => r.User)
				.SingleOrDefaultAsync(p => p.Id == publicationId);

			if (publication == null)
			{
				return null;
			}

			var model = new PublicationRatingsModel
			{
				PublicationId = publication.Id,
				PublicationTitle = publication.Title,
				Ratings = publication.PublicationRatings
					.GroupBy(
						key => new { key.PublicationId, key.User.UserName, key.User.PublicRatings },
						grp => new { grp.Type, grp.Value })
					.Select(g => new PublicationRatingsModel.RatingEntry
					{
						UserName = g.Key.UserName,
						IsPublic = g.Key.PublicRatings,
						Entertainment = g.FirstOrDefault(v => v.Type == PublicationRatingType.Entertainment)?.Value,
						TechQuality = g.FirstOrDefault(v => v.Type == PublicationRatingType.TechQuality)?.Value
					})
					.ToList()
			};

			model.AverageEntertainmentRating = Math.Round(
				model.Ratings
					.Where(r => r.Entertainment.HasValue)
					.Select(r => r.Entertainment.Value)
					.Average(), 
				2);

			model.AverageTechRating = Math.Round(
				model.Ratings
					.Where(r => r.TechQuality.HasValue)
					.Select(r => r.TechQuality.Value)
					.Average(), 
				2);

			// Entertainment counts 2:1 over Tech
			model.OverallRating = Math.Round(
				model.Ratings
					.Where(r => r.Entertainment.HasValue)
					.Select(r => r.Entertainment.Value)
					.Concat(model.Ratings
						.Where(r => r.Entertainment.HasValue)
						.Select(r => r.Entertainment.Value))
					.Concat(model.Ratings
						.Where(r => r.TechQuality.HasValue)
						.Select(r => r.TechQuality.Value))
					.Average(), 
				2);

			_cache.Set(MovieRatingKey + publicationId, model);

			return model;
		}

		/// <summary>
		/// Returns the overall rating for the <see cref="Publication"/> with the given <see cref="publicationIds"/>
		/// </summary>
		/// <exception cref="ArgumentException">If <see cref="publicationIds"/> is null</exception>
		public async Task<IDictionary<int, double?>> GetOverallRatingsForPublications(IEnumerable<int> publicationIds)
		{
			return (await _pointsService.CalculatePublicationRatings(publicationIds))
				.ToDictionary(
					tkey => tkey.Key,
					tvalue => tvalue.Value.Overall);
		}
	}
}
