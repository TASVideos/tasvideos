using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TASVideos.Core.Services;
using TASVideos.Data.Entity;

namespace TASVideos.Pages.Users;

[AllowAnonymous]
public class RatingsModel(UserManager userManager) : BasePageModel
{
	[FromRoute]
	public string UserName { get; set; } = "";

	public UserRatings Ratings { get; set; } = new();

	public async Task<IActionResult> OnGet()
	{
		var ratings = await userManager.GetUserRatings(
			UserName,
			User.Has(PermissionTo.SeePrivateRatings) || User.Name() == UserName);

		if (ratings is null)
		{
			return NotFound();
		}

		Ratings = ratings;
		return Page();
	}
}
