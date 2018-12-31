using System.Threading.Tasks;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

using TASVideos.Data.Entity;
using TASVideos.Models;
using TASVideos.Tasks;

namespace TASVideos.Pages.Profile
{
	[Authorize]
	public class IndexModel : BasePageModel
	{
		private readonly UserManager<User> _userManager;
		private readonly AwardTasks _awardTasks;

		public IndexModel(
			UserManager<User> userManager,
			AwardTasks awardTasks,
			UserTasks userTasks)
			: base(userTasks)
		{
			_userManager = userManager;
			_awardTasks = awardTasks;
		}

		public UserProfileModel Profile { get; set; } = new UserProfileModel();

		public async Task<IActionResult> OnGet()
		{
			var userName = _userManager.GetUserName(User);
			Profile = await UserTasks.GetUserProfile(userName, includeHidden: true);
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
