using System.Threading.Tasks;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

using TASVideos.Data.Entity;
using TASVideos.Models;
using TASVideos.Services;

namespace TASVideos.Pages.Users
{
	[AllowAnonymous]
	public class RatingsModel : BasePageModel
	{
		private readonly UserManager _userManager;

		public RatingsModel(
			UserManager userManager)
			: base(userManager)
		{
			_userManager = userManager;
		}

		[FromRoute]
		public string UserName { get; set; }

		public UserRatingsModel Ratings { get; set; } = new UserRatingsModel();

		public async Task<IActionResult> OnGet()
		{
			Ratings = await _userManager.GetUserRatings(
				UserName,
				User.Has(PermissionTo.SeePrivateRatings));

			if (Ratings == null)
			{
				return NotFound();
			}

			return Page();
		}
	}
}
