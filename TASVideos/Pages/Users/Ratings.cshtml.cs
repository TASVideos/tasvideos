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

	public UserRatings? Ratings { get; set; }

	public async Task<IActionResult> OnGet()
	{
		Ratings = await userManager.GetUserRatings(
			UserName,
			User.Has(PermissionTo.SeePrivateRatings) || User.Name() == UserName);

		if (Ratings is null)
		{
			return NotFound();
		}

		return Page();
	}
}
