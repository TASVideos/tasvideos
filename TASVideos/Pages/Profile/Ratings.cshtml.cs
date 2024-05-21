namespace TASVideos.Pages.Profile;

[Authorize]
public class RatingsModel(UserManager userManager) : BasePageModel
{
	[FromQuery]
	public RatingRequest Search { get; set; } = new();

	public UserRatings Ratings { get; set; } = new();

	public async Task OnGet()
	{
		Ratings = (await userManager.GetUserRatings(User.Name(), Search, includeHidden: true))!;
	}
}
