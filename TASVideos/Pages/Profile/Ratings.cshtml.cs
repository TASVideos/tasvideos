using System.Threading.Tasks;

using Microsoft.AspNetCore.Authorization;

using TASVideos.Models;
using TASVideos.Tasks;

namespace TASVideos.Pages.Profile
{
	[Authorize]
	public class RatingsModel : BasePageModel
	{
		public RatingsModel(UserTasks userTasks) : base(userTasks)
		{
		}

		public UserRatingsModel Ratings { get; set; } = new UserRatingsModel();

		public async Task OnGet()
		{
			Ratings = await UserTasks.GetUserRatings(User.Identity.Name, includeHidden: true);
		}
	}
}
