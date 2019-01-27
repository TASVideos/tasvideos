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
		private readonly AwardTasks _awardTasks;

		public IndexModel(
			AwardTasks awardTasks,
			UserTasks userTasks)
			: base(userTasks)
		{
			_awardTasks = awardTasks;
		}

		public UserProfileModel Profile { get; set; } = new UserProfileModel();

		public async Task<IActionResult> OnGet()
		{
			Profile = await UserTasks.GetUserProfile(User.Identity.Name, includeHidden: true);
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
