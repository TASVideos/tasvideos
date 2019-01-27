using System.Threading.Tasks;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

using TASVideos.Models;
using TASVideos.Services;
using TASVideos.Tasks;

namespace TASVideos.Pages.Profile
{
	[Authorize]
	public class IndexModel : BasePageModel
	{
		private readonly AwardTasks _awardTasks;
		private readonly UserManager _userManager;

		public IndexModel(
			AwardTasks awardTasks,
			UserManager userManager,
			UserTasks userTasks)
			: base(userTasks)
		{
			_awardTasks = awardTasks;
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

			Profile.Awards = await _awardTasks.GetAllAwardsForUser(Profile.Id);

			return Page();
		}
	}
}
