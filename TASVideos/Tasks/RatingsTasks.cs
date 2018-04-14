using System;
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
		private readonly ApplicationDbContext _db;
		private readonly ICacheService _cache;

		private const string MovieRatingKey = "OverallRatingForMovie-";

		public RatingsTasks(
			ApplicationDbContext db,
			ICacheService cache)
		{
			_db = db;
			_cache = cache;
		}

		/// <summary>
		/// Returns a detailed list of all ratings for a <see cref="Publication"/>
		/// with the given <see cref="publicationId"/>
		/// </summary>
		public async Task<PublicationRatingsViewModel> GetRatingsForPublication(int publicationId)
		{
			var publication = await _db.Publications
				.Include(p => p.PublicationRatings)
				.ThenInclude(r => r.User)
				.SingleOrDefaultAsync(p => p.Id == publicationId);

			if (publication == null)
			{
				return null;
			}

			var model = new PublicationRatingsViewModel
			{
				PublicationId = publication.Id,
				PublicationTitle = publication.Title,
				Ratings = publication.PublicationRatings
					.GroupBy(
						key => new { key.PublicationId, key.User.UserName, key.User.PublicRatings },
						grp => new { grp.Type, grp.Value })
					.Select(g => new PublicationRatingsViewModel.RatingEntry
					{
						UserName = g.Key.UserName,
						IsPublic = g.Key.PublicRatings,
						Entertainment = g.FirstOrDefault(v => v.Type == PublicationRatingType.Entertainment)?.Value,
						TechQuality = g.FirstOrDefault(v => v.Type == PublicationRatingType.TechQuality)?.Value
					})
					.ToList()
			};

			_cache.Set(MovieRatingKey + publicationId, model.OverallRating);

			return model;
		}
	}
}
