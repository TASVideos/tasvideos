namespace TASVideos.Pages.Profile;

[Authorize]
public class RatingsModel(IRatingService ratingService) : BasePageModel
{
	[FromQuery]
	public RatingRequest Search { get; set; } = new();

	public UserRatings Ratings { get; set; } = new();

	public async Task OnGet()
	{
		Ratings = (await ratingService.GetUserRatings(User.Name(), Search, includeHidden: true))!;
	}
}
