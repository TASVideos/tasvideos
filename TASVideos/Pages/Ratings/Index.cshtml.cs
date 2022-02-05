﻿using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TASVideos.Core.Services;
using TASVideos.Data;
using TASVideos.Data.Entity;
using TASVideos.Pages.Ratings.Models;

namespace TASVideos.Pages.Ratings;

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

	public PublicationRatingsModel Publication { get; set; } = new();

	public IEnumerable<PublicationRatingsModel.RatingEntry> VisibleRatings => User.Has(PermissionTo.SeePrivateRatings)
		? Publication.Ratings
		: Publication.Ratings.Where(r => r.IsPublic || r.UserName == User.Name());

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
					key => new { key.PublicationId, key.User!.UserName, key.User.PublicRatings },
					grp => new { grp.Type, grp.Value })
				.Select(g => new PublicationRatingsModel.RatingEntry
				{
					UserName = g.Key.UserName,
					IsPublic = g.Key.PublicRatings,
					Rating = g.FirstOrDefault(v => v.Type == PublicationRatingType.Entertainment)?.Value
				})
				.ToList()
		};

		var entertainmentRatings = Publication.Ratings
			.Where(r => r.Rating.HasValue)
			.Select(r => r.Rating!.Value)
			.ToList();

		// Entertainment counts 2:1 over Tech
		Publication.OverallRating = entertainmentRatings.Any()
			? Math.Round(entertainmentRatings.Average(), 2)
			: 0;

		_cache.Set(CacheKeys.MovieRatingKey + Id, Publication);

		return Page();
	}
}
