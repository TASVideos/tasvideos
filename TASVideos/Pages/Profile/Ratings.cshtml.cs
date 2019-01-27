using System.Threading.Tasks;

using Microsoft.AspNetCore.Authorization;

using TASVideos.Models;
using TASVideos.Services;
using TASVideos.Tasks;

namespace TASVideos.Pages.Profile
{
	[Authorize]
	public class RatingsModel : BasePageModel
	{
		private readonly UserManager _userManager;
		public RatingsModel(
			UserManager userManager,
			UserTasks userTasks) 
			: base(userTasks)
		{
			_userManager = userManager;
		}

		public UserRatingsModel Ratings { get; set; } = new UserRatingsModel();

		public async Task OnGet()
		{
			Ratings = await _userManager.GetUserRatings(User.Identity.Name, includeHidden: true);
		}
	}
}
