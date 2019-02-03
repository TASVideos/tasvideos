using System.Threading.Tasks;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

using TASVideos.Models;
using TASVideos.Services;

namespace TASVideos.Pages.Profile
{
	[Authorize]
	public class IndexModel : BasePageModel
	{
		private readonly IAwardsCache _awards;
		private readonly UserManager _userManager;

		public IndexModel(
			IAwardsCache awards,
			UserManager userManager)
		{
			_awards = awards;
			_userManager = userManager;
		}

		public UserProfileModel Profile { get; set; } = new UserProfileModel();

		public async Task<IActionResult> OnGet()
		{
			Profile = await _userManager.GetUserProfile(User.Identity.Name, includeHidden: true);
			if (Profile == null)
			{
				return NotFound();
			}

			if (!string.IsNullOrWhiteSpace(Profile.Signature))
			{
				Profile.Signature = RenderPost(Profile.Signature, true, false);
			}

			Profile.Awards = await _awards.AwardsForUser(Profile.Id);

			return Page();
		}
	}
}
