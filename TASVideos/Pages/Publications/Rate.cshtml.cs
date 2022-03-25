using System.Globalization;
using System.Text.Json;
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
		if (publication is null)
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
			Rating = ratings
				.FirstOrDefault()
				?.Value.ToString(CultureInfo.InvariantCulture)
		};

		Rating.Unrated = Rating.Rating is null;

		return Page();
	}

	public async Task<IActionResult> OnPost()
	{
		if (!ModelState.IsValid)
		{
			return Page();
		}

		var userId = User.GetUserId();

		var rating = await _db.PublicationRatings
			.ForPublication(Id)
			.ForUser(userId)
			.FirstOrDefaultAsync();

		UpdateRating(rating, Id, userId, PublicationRateModel.RatingString.AsRatingDouble(Rating.Rating), Rating.Unrated);

		await _db.SaveChangesAsync();

		return BasePageRedirect("/Ratings/Index", new { Id });
	}

	public async Task<IActionResult> OnPostInline()
	{
		Rating = await JsonSerializer.DeserializeAsync<PublicationRateModel>(Request.Body) ?? Rating;
		ModelState.ClearValidationState(nameof(Rating));
		if (!TryValidateModel(Rating, nameof(Rating)))
		{
			return new ContentResult { StatusCode = StatusCodes.Status400BadRequest };
		}

		var userId = User.GetUserId();
		var rating = await _db.PublicationRatings
			.ForPublication(Id)
			.ForUser(userId)
			.FirstOrDefaultAsync();
		UpdateRating(rating, Id, userId, PublicationRateModel.RatingString.AsRatingDouble(Rating.Rating), Rating.Unrated);

		await _db.SaveChangesAsync();

		var updatedRatings = await _db.Publications
			.Where(p => p.Id == Id)
			.Select(p => new
			{
				RatingCount = p.PublicationRatings.Count,
				OverallRating = (double?)p.PublicationRatings
						.Where(pr => !pr.Publication!.Authors.Select(a => a.UserId).Contains(pr.UserId))
						.Where(pr => pr.User!.UseRatings)
						.Average(pr => pr.Value),
			})
			.Select(rro => new
			{
				Rating.Rating,
				rro.RatingCount,
				OverallRating = (rro.OverallRating ?? 0).ToOverallRatingString(),
			})
			.SingleOrDefaultAsync();

		return new ContentResult
		{
			StatusCode = StatusCodes.Status200OK,
			Content = JsonSerializer.Serialize(updatedRatings)
		};
	}

	private void UpdateRating(PublicationRating? rating, int id, int userId, double? value, bool remove)
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
					Type = PublicationRatingType.Entertainment,
					Value = Math.Round(value.Value, 1)
				});
			}

			// Else do nothing
		}
	}
}
