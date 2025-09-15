namespace TASVideos.Pages.Users;

[AllowAnonymous]
public class ProfileModel(IAwards awards, IUserManager userManager) : BasePageModel
{
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
		var profile = await userManager.GetUserProfile(UserName, false, seeRestricted);
		if (profile is null)
		{
			return NotFound();
		}

		Profile = profile;
		Profile.Awards = await awards.ForUser(Profile.Id);
		return Page();
	}
}
