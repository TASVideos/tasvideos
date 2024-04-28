using System.Globalization;
using System.Text.Json;

namespace TASVideos.Pages.Publications;

[RequirePermission(PermissionTo.RateMovies)]
public class RateModel(ApplicationDbContext db) : BasePageModel
{
	[FromRoute]
	public int Id { get; set; }

	[BindProperty]
	public RatingDisplay Rating { get; set; } = new();

	public List<RatingEntry> AllRatings = [];
	public double OverallRating { get; set; }

	public IEnumerable<RatingEntry> VisibleRatings => User.Has(PermissionTo.SeePrivateRatings)
		? AllRatings
		: AllRatings.Where(r => r.IsPublic || r.UserName == User.Name());

	public async Task<IActionResult> OnGet()
	{
		var userId = User.GetUserId();
		var publication = await db.Publications
			.Include(p => p.PublicationRatings)
			.ThenInclude(r => r.User)
			.SingleOrDefaultAsync(p => p.Id == Id);
		if (publication is null)
		{
			return NotFound();
		}

		var ratings = await db.PublicationRatings
			.ForPublication(Id)
			.ForUser(userId)
			.ToListAsync();

		Rating = new RatingDisplay
		{
			Title = publication.Title,
			Rating = ratings
				.FirstOrDefault()
				?.Value.ToString(CultureInfo.InvariantCulture)
		};

		Rating.Unrated = Rating.Rating is null;

		AllRatings = publication.PublicationRatings
			.Select(pr => new RatingEntry(pr.User!.UserName, pr.Value, pr.User!.PublicRatings))
			.ToList();

		OverallRating = AllRatings.Any()
			? Math.Round(AllRatings.Select(r => r.Rating).Average(), 2)
			: 0;

		return Page();
	}

	public async Task<IActionResult> OnPost()
	{
		if (!ModelState.IsValid)
		{
			return Page();
		}

		var userId = User.GetUserId();

		var rating = await db.PublicationRatings
			.ForPublication(Id)
			.ForUser(userId)
			.FirstOrDefaultAsync();

		UpdateRating(rating, Id, userId, RatingDisplay.RatingString.AsRatingDouble(Rating.Rating), Rating.Unrated);

		await ConcurrentSave(db, $"{Rating.Title} successfully rated.", $"Unable to rate {Rating.Title}");

		return BasePageRedirect("/Publications/Rate", new { Id });
	}

	public async Task<IActionResult> OnPostInline()
	{
		Rating = await JsonSerializer.DeserializeAsync<RatingDisplay>(Request.Body) ?? Rating;
		ModelState.ClearValidationState(nameof(Rating));
		if (!TryValidateModel(Rating, nameof(Rating)))
		{
			return new ContentResult { StatusCode = StatusCodes.Status400BadRequest };
		}

		var userId = User.GetUserId();
		var ratingObject = await db.PublicationRatings
			.ForPublication(Id)
			.ForUser(userId)
			.FirstOrDefaultAsync();
		var ratingValue = RatingDisplay.RatingString.AsRatingDouble(Rating.Rating);
		UpdateRating(ratingObject, Id, userId, ratingValue, Rating.Unrated);

		await db.SaveChangesAsync();

		var updatedRatings = await db.Publications
			.Where(p => p.Id == Id)
			.Select(p => new
			{
				RatingCount = p.PublicationRatings.Count,
				OverallRating = (double?)p.PublicationRatings
					.Where(pr => !pr.Publication!.Authors.Select(a => a.UserId).Contains(pr.UserId))
					.Where(pr => pr.User!.UseRatings)
					.Average(pr => pr.Value)
			})
			.Select(rro => new
			{
				Rating = ratingValue.ToString(),
				rro.RatingCount,
				OverallRating = (rro.OverallRating ?? 0).ToOverallRatingString()
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
				db.PublicationRatings.Remove(rating);
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
				db.PublicationRatings.Add(new PublicationRating
				{
					PublicationId = id,
					UserId = userId,
					Value = Math.Round(value.Value, 1)
				});
			}

			// Else do nothing
		}
	}

	public record RatingEntry(string UserName, double Rating, bool IsPublic);

	public class RatingDisplay
	{
		public sealed class RatingString : ValidationAttribute
		{
			public static double? AsRatingDouble(string? ratingString)
			{
				NumberFormatInfo customFormat = new CultureInfo("en-US").NumberFormat;
				customFormat.NumberDecimalSeparator = ".";
				var result = double.TryParse(ratingString, NumberStyles.AllowDecimalPoint, customFormat, out double ratingNumber);
				if (!result)
				{
					customFormat.NumberDecimalSeparator = ",";
					result = double.TryParse(ratingString, NumberStyles.AllowDecimalPoint, customFormat, out ratingNumber);
					if (!result)
					{
						return null;
					}
				}

				ratingNumber = Math.Round(ratingNumber, 1, MidpointRounding.AwayFromZero);

				return ratingNumber;
			}

			public override bool IsValid(object? value)
			{
				var ratingString = value as string;
				if (string.IsNullOrWhiteSpace(ratingString))
				{
					return true;
				}

				var ratingNumber = AsRatingDouble(ratingString);
				return ratingNumber is >= 0.0 and <= 10.0;
			}
		}

		public string Title { get; init; } = "";

		[RatingString(ErrorMessage = "{0} must be between 0 and 10.")]
		public string? Rating { get; init; }

		public bool Unrated { get; set; }
	}
}
