using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TASVideos.Core.Services;
using TASVideos.Data.Entity;

namespace TASVideos.Pages.Profile;

[Authorize]
public class IndexModel : BasePageModel
{
	private readonly IAwards _awards;
	private readonly UserManager _userManager;

	public IndexModel(
		IAwards awards,
		UserManager userManager)
	{
		_awards = awards;
		_userManager = userManager;
	}

	public UserProfile Profile { get; set; } = new();

	public async Task<IActionResult> OnGet()
	{
		var seeRestricted = User.Has(PermissionTo.SeeRestrictedForums);
		var profile = await _userManager.GetUserProfile(User!.Identity!.Name!, true, seeRestricted);
		if (profile == null)
		{
			return NotFound();
		}

		Profile = profile;
		Profile.Awards = await _awards.ForUser(Profile.Id);

		return Page();
	}
}
