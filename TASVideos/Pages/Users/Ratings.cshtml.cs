namespace TASVideos.Pages.Users;

[AllowAnonymous]
public class RatingsModel(IRatingService ratingService) : BasePageModel
{
	[FromQuery]
	public RatingRequest Search { get; set; } = new();

	[FromRoute]
	public string UserName { get; set; } = "";

	public UserRatings Ratings { get; set; } = new();

	public async Task<IActionResult> OnGet()
	{
		var ratings = await ratingService.GetUserRatings(
			UserName,
			Search,
			User.Has(PermissionTo.SeePrivateRatings) || User.Name() == UserName);

		if (ratings is null)
		{
			return NotFound();
		}

		Ratings = ratings;
		return Page();
	}
}
