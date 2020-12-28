using System.Threading.Tasks;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

using TASVideos.Data.Entity;
using TASVideos.Models;
using TASVideos.Services;

namespace TASVideos.Pages.Profile
{
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

		public UserProfileModel Profile { get; set; } = new ();

		public async Task<IActionResult> OnGet()
		{
			var seeRestricted = User.Has(PermissionTo.SeeRestrictedForums);
			var profile = await _userManager.GetUserProfile(User!.Identity!.Name!, true, seeRestricted);
			if (profile == null)
			{
				return NotFound();
			}

			Profile = profile;
			if (!string.IsNullOrWhiteSpace(Profile.Signature))
			{
				Profile.Signature = RenderPost(Profile.Signature, true, false);
			}

			Profile.Awards = await _awards.ForUser(Profile.Id);

			return Page();
		}
	}
}
