using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

using TASVideos.Data;
using TASVideos.Data.Entity;
using TASVideos.Pages.Ratings.Models;
using TASVideos.Services;

namespace TASVideos.Pages.Ratings
{
	[AllowAnonymous]
	public class IndexModel : BasePageModel
	{
		private readonly ApplicationDbContext _db;
		private readonly ICacheService _cache;

		public IndexModel(
			ApplicationDbContext db,
			ICacheService cache)
		{
			_db = db;
			_cache = cache;
		}

		[FromRoute]
		public int Id { get; set; }

		public PublicationRatingsModel Publication { get; set; }

		public IEnumerable<PublicationRatingsModel.RatingEntry> VisibleRatings => User.Has(PermissionTo.SeePrivateRatings)
			? Publication.Ratings
			: Publication.Ratings.Where(r => r.IsPublic);

		// TODO: refactor to use pointsService for calculations
		public async Task<IActionResult> OnGet()
		{
			string cacheKey = CacheKeys.MovieRatingKey + Id;
			if (_cache.TryGetValue(cacheKey, out PublicationRatingsModel rating))
			{
				Publication = rating;
			}

			var publication = await _db.Publications
				.Include(p => p.PublicationRatings)
				.ThenInclude(r => r.User)
				.SingleOrDefaultAsync(p => p.Id == Id);

			if (publication == null)
			{
				return NotFound();
			}

			Publication = new PublicationRatingsModel
			{
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

			var entertainmentRatings = Publication.Ratings
				.Where(r => r.Entertainment.HasValue)
				.Select(r => r.Entertainment.Value)
				.ToList();

			var techRatings = Publication.Ratings
				.Where(r => r.TechQuality.HasValue)
				.Select(r => r.TechQuality.Value)
				.ToList();

			var overallRatings = entertainmentRatings
				.Concat(techRatings)
				.ToList();

			Publication.AverageEntertainmentRating = entertainmentRatings.Any() 
				? Math.Round(entertainmentRatings.Average(), 2)
				: 0;

			Publication.AverageTechRating = techRatings.Any()
				? Math.Round(techRatings.Average(), 2)
				: 0;

			// Entertainment counts 2:1 over Tech
			Publication.OverallRating = overallRatings.Any()
				? Math.Round(overallRatings.Average(), 2)
				: 0;

			_cache.Set(CacheKeys.MovieRatingKey + Id, Publication);

			return Page();
		}
	}
}
