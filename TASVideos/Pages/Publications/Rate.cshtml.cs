using System.Globalization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TASVideos.Data;
using TASVideos.Data.Entity;
using TASVideos.Pages.Publications.Models;

namespace TASVideos.Pages.Publications;

[RequirePermission(PermissionTo.RateMovies)]
public class RateModel : BasePageModel
{
	private readonly ApplicationDbContext _db;

	public RateModel(ApplicationDbContext db)
	{
		_db = db;
	}

	[FromRoute]
	public int Id { get; set; }

	[BindProperty]
	public PublicationRateModel Rating { get; set; } = new();

	public async Task<IActionResult> OnGet()
	{
		var userId = User.GetUserId();
		var publication = await _db.Publications.SingleOrDefaultAsync(p => p.Id == Id);
		if (publication == null)
		{
			return NotFound();
		}

		var ratings = await _db.PublicationRatings
			.ForPublication(Id)
			.ForUser(userId)
			.ToListAsync();

		Rating = new PublicationRateModel
		{
			Title = publication.Title,
			TechRating = ratings
				.SingleOrDefault(r => r.Type == PublicationRatingType.TechQuality)
				?.Value.ToString(CultureInfo.InvariantCulture),
			EntertainmentRating = ratings
				.SingleOrDefault(r => r.Type == PublicationRatingType.Entertainment)
				?.Value.ToString(CultureInfo.InvariantCulture)
		};

		Rating.TechUnrated = Rating.TechRating == null;
		Rating.EntertainmentUnrated = Rating.EntertainmentRating == null;

		return Page();
	}

	public async Task<IActionResult> OnPost()
	{
		if (!ModelState.IsValid)
		{
			return Page();
		}

		var userId = User.GetUserId();

		var ratings = await _db.PublicationRatings
			.ForPublication(Id)
			.ForUser(userId)
			.ToListAsync();

		var tech = ratings
			.SingleOrDefault(r => r.Type == PublicationRatingType.TechQuality);

		var entertainment = ratings
			.SingleOrDefault(r => r.Type == PublicationRatingType.Entertainment);

		UpdateRating(tech, Id, userId, PublicationRatingType.TechQuality, PublicationRateModel.RatingString.AsRatingDouble(Rating.TechRating), Rating.TechUnrated);
		UpdateRating(entertainment, Id, userId, PublicationRatingType.Entertainment, PublicationRateModel.RatingString.AsRatingDouble(Rating.EntertainmentRating), Rating.EntertainmentUnrated);

		await _db.SaveChangesAsync();

		return BasePageRedirect("/Ratings/Index", new { Id });
	}

	private void UpdateRating(PublicationRating? rating, int id, int userId, PublicationRatingType type, double? value, bool remove)
	{
		if (rating is not null)
		{
			if (remove)
			{
				// Remove
				_db.PublicationRatings.Remove(rating);
			}
			else if (value.HasValue)
			{
				// Update
				rating.Value = value.Value;
			}
		}
		else
		{
			if (value.HasValue && !remove)
			{
				// Add
				_db.PublicationRatings.Add(new PublicationRating
				{
					PublicationId = id,
					UserId = userId,
					Type = type,
					Value = Math.Round(value.Value, 1)
				});
			}

			// Else do nothing
		}
	}
}
