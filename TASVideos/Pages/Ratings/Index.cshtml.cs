using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

using TASVideos.Data;
using TASVideos.Data.Entity;
using TASVideos.Models;
using TASVideos.Services;

namespace TASVideos.Pages.Ratings
{
	[AllowAnonymous]
	public class IndexModel : BasePageModel
	{
		// TODO: move to a constants file
		private const string MovieRatingKey = "OverallRatingForMovieViewModel-";

		private readonly ApplicationDbContext _db;
		private readonly ICacheService _cache;

		public IndexModel(
			ApplicationDbContext db,
			ICacheService cache,
			UserManager userManager) 
			: base(userManager)
		{
			_db = db;
			_cache = cache;
		}

		[FromRoute]
		public int Id { get; set; }

		public PublicationRatingsModel Publication { get; set; } = new PublicationRatingsModel();

		public IEnumerable<PublicationRatingsModel.RatingEntry> VisibleRatings => UserHas(PermissionTo.SeePrivateRatings)
			? Publication.Ratings
			: Publication.Ratings.Where(r => r.IsPublic);

		public async Task<IActionResult> OnGet()
		{
			Publication = await GetRatingsForPublication(Id);
			if (Publication == null)
			{
				return NotFound();
			}

			return Page();
		}

		// TODO: refactor to use pointsService for calculations
		// TODO: move at least some of this logic inline
		/// <summary>
		/// Returns a detailed list of all ratings for a <see cref="Publication"/>
		/// with the given <see cref="publicationId"/>
		/// If no <see cref="Publication"/> is found, then null is returned
		/// </summary>
		private async Task<PublicationRatingsModel> GetRatingsForPublication(int publicationId)
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
	}
}
