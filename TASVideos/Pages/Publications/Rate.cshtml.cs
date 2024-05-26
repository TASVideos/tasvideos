using System.Text.Json;

namespace TASVideos.Pages.Publications;

[RequirePermission(PermissionTo.RateMovies)]
public class RateModel(ApplicationDbContext db, IRatingService ratingService) : BasePageModel
{
	[FromRoute]
	public int Id { get; set; }

	[BindProperty]
	public string? Title { get; set; } = "";

	[BindProperty]
	[RatingString(ErrorMessage = "{0} must be between 0 and 10.")]
	public string? Rating { get; set; }

	public ICollection<RatingEntry> AllRatings = [];
	public double OverallRating { get; set; }

	public IEnumerable<RatingEntry> VisibleRatings => User.Has(PermissionTo.SeePrivateRatings)
		? AllRatings
		: AllRatings.Where(r => r.IsPublic || r.UserName == User.Name());

	public async Task<IActionResult> OnGet()
	{
		var userId = User.GetUserId();
		var title = await db.Publications
			.Where(p => p.Id == Id)
			.Select(p => p.Title)
			.SingleOrDefaultAsync();

		if (title is null)
		{
			return NotFound();
		}

		Title = title;
		Rating = (await ratingService.GetUserRatingForPublication(userId, Id))?.Value.ToOverallRatingString();
		AllRatings = await ratingService.GetRatingsForPublication(Id);
		OverallRating = await ratingService.GetOverallRatingForPublication(Id);

		return Page();
	}

	public async Task<IActionResult> OnPost()
	{
		if (!ModelState.IsValid)
		{
			return Page();
		}

		var ratingValue = RatingString.AsRatingDouble(Rating);
		var result = await ratingService.UpdateUserRating(User.GetUserId(), Id, ratingValue);
		SetMessage(result, $"{Title} successfully rated.", $"Unable to rate {Title}");

		return BasePageRedirect("/Publications/Rate", new { Id });
	}

	public async Task<IActionResult> OnPostInline()
	{
		Rating = await JsonSerializer.DeserializeAsync<string?>(Request.Body);
		ModelState.ClearValidationState(nameof(Rating));
		if (Rating is not null && !TryValidateModel(Rating, nameof(Rating)))
		{
			return BadRequest();
		}

		var ratingValue = RatingString.AsRatingDouble(Rating);
		await ratingService.UpdateUserRating(User.GetUserId(), Id, ratingValue);

		OverallRating = await ratingService.GetOverallRatingForPublication(Id);

		return Json(new { OverallRating = OverallRating.ToOverallRatingString() });
	}
}
