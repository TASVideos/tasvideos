using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TASVideos.Core.Services;
using TASVideos.Data.Entity;

namespace TASVideos.Pages.Users;

[AllowAnonymous]
public class ProfileModel : BasePageModel
{
	private readonly IAwards _awards;
	private readonly UserManager _userManager;

	public ProfileModel(
		IAwards awards,
		UserManager userManager)
	{
		_awards = awards;
		_userManager = userManager;
	}

	// Allows for a query based call to this page for Users/List
	[FromQuery]
	public string? Name { get; set; }

	[FromRoute]
	public string? UserName { get; set; }

	public UserProfile Profile { get; set; } = new();

	public async Task<IActionResult> OnGet()
	{
		if (string.IsNullOrWhiteSpace(UserName) && string.IsNullOrWhiteSpace(Name))
		{
			return BasePageRedirect("/Users/List");
		}

		if (string.IsNullOrWhiteSpace(UserName))
		{
			UserName = Name ?? "";
		}

		var seeRestricted = User.Has(PermissionTo.SeeRestrictedForums);
		var profile = await _userManager.GetUserProfile(UserName, false, seeRestricted);
		if (profile is null)
		{
			return NotFound();
		}

		Profile = profile;
		Profile.Awards = await _awards.ForUser(Profile.Id);
		return Page();
	}
}
