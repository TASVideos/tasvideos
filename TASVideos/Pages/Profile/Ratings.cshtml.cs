namespace TASVideos.Pages.Profile;

[Authorize]
public class RatingsModel(UserManager userManager) : BasePageModel
{
	public UserRatings Ratings { get; set; } = new();

	public async Task OnGet()
	{
		Ratings = (await userManager.GetUserRatings(User.Name(), includeHidden: true))!;
	}
}
