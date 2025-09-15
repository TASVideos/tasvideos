namespace TASVideos.Pages.Profile;

[Authorize]
public class IndexModel(IAwards awards, IUserManager userManager) : BasePageModel
{
	public UserProfile Profile { get; set; } = new();

	public async Task<IActionResult> OnGet()
	{
		var seeRestricted = User.Has(PermissionTo.SeeRestrictedForums);
		var profile = await userManager.GetUserProfile(User.Name(), true, seeRestricted);
		if (profile is null)
		{
			return NotFound();
		}

		Profile = profile;
		Profile.Awards = await awards.ForUser(Profile.Id);

		return Page();
	}
}
