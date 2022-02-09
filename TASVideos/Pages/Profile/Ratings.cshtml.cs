using Microsoft.AspNetCore.Authorization;
using TASVideos.Core.Services;

namespace TASVideos.Pages.Profile;

[Authorize]
public class RatingsModel : BasePageModel
{
	private readonly UserManager _userManager;

	public RatingsModel(UserManager userManager)
	{
		_userManager = userManager;
	}

	public UserRatings Ratings { get; set; } = new();

	public async Task OnGet()
	{
		Ratings = (await _userManager.GetUserRatings(User.Name(), includeHidden: true))!;
	}
}
