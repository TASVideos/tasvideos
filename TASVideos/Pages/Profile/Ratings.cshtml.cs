using System.Threading.Tasks;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;

using TASVideos.Data.Entity;
using TASVideos.Models;
using TASVideos.Tasks;

namespace TASVideos.Pages.Profile
{
	[Authorize]
	public class RatingsModel : BasePageModel
	{
		private readonly UserManager<User> _userManager;

		public RatingsModel(
			UserManager<User> userManager,
			UserTasks userTasks) 
			: base(userTasks)
		{
			_userManager = userManager;
		}

		public UserRatingsModel Ratings { get; set; } = new UserRatingsModel();

		public async Task OnGet()
		{
			var user = await _userManager.GetUserAsync(User);
			Ratings = await UserTasks.GetUserRatings(user.UserName, includeHidden: true);
		}
	}
}
