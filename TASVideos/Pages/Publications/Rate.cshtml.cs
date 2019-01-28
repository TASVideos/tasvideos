using System.Linq;
using System.Threading.Tasks;

using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TASVideos.Data;
using TASVideos.Data.Entity;
using TASVideos.Models;

namespace TASVideos.Pages.Publications
{
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
		
		[FromQuery]
		public string ReturnUrl { get; set; }

		[BindProperty]
		public PublicationRateModel Rating { get; set; }

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
					?.Value,
				EntertainmentRating = ratings
					.SingleOrDefault(r => r.Type == PublicationRatingType.Entertainment)
					?.Value
			};

			if (Rating == null)
			{
				return NotFound();
			}

			return Page();
		}

		public async Task<IActionResult> OnPost()
		{
			if (!Rating.EntertainmentRating.HasValue && !Rating.TechRating.HasValue)
			{
				ModelState.AddModelError("", "At least one rating must be set");
			}

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

			UpdateRating(tech, Id, userId, PublicationRatingType.TechQuality, Rating.TechRating);
			UpdateRating(entertainment, Id, userId, PublicationRatingType.Entertainment, Rating.EntertainmentRating);

			await _db.SaveChangesAsync();

			if (!string.IsNullOrWhiteSpace(ReturnUrl))
			{
				return RedirectToLocal(ReturnUrl);
			}

			return RedirectToPage("/Profile/Ratings");
		}

		private void UpdateRating(PublicationRating rating, int id, int userId, PublicationRatingType type, double? value)
		{
			if (rating != null)
			{
				if (value.HasValue)
				{
					// Update
					rating.Value = value.Value;
				}
				else
				{
					// Remove
					_db.PublicationRatings.Remove(rating);
				}
			}
			else
			{
				if (value.HasValue)
				{
					// Add
					_db.PublicationRatings.Add(new PublicationRating
					{
						PublicationId = id,
						UserId = userId,
						Type = type,
						Value = value.Value
					});
				}

				// Else do nothing
			}
		}
	}
}
