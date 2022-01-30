using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TASVideos.Core.Services;
using TASVideos.Data.Entity;

namespace TASVideos.Pages.Users;

[AllowAnonymous]
public class RatingsModel : BasePageModel
{
	private readonly UserManager _userManager;

	public RatingsModel(UserManager userManager)
	{
		_userManager = userManager;
	}

	[FromRoute]
	public string UserName { get; set; } = "";

	public UserRatings? Ratings { get; set; }

	public async Task<IActionResult> OnGet()
	{
		Ratings = await _userManager.GetUserRatings(
			UserName,
			User.Has(PermissionTo.SeePrivateRatings));

		if (Ratings == null)
		{
			return NotFound();
		}

		return Page();
	}
}
