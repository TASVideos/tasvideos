using System.Threading.Tasks;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

using TASVideos.Data.Entity;
using TASVideos.Models;
using TASVideos.Tasks;

namespace TASVideos.Pages.Users
{
	[AllowAnonymous]
	public class RatingsModel : BasePageModel
	{
		public RatingsModel(UserTasks userTasks) : base(userTasks)
		{
		}

		[FromRoute]
		public string UserName { get; set; }

		public UserRatingsModel Ratings { get; set; } = new UserRatingsModel();

		public async Task<IActionResult> OnGet()
		{
			Ratings = await UserTasks.GetUserRatings(
				UserName,
				UserHas(PermissionTo.SeePrivateRatings));
			if (Ratings == null)
			{
				return NotFound();
			}

			return Page();
		}
	}
}
