using System.Threading.Tasks;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

using TASVideos.Models;
using TASVideos.Services;

namespace TASVideos.Pages.Users
{
	[AllowAnonymous]
	public class ProfileModel : BasePageModel
	{
		private readonly IAwards _awards;
		private readonly UserManager _userManager;

		public ProfileModel(
			IAwards awards,
			UserManager userManager)
		{
			_awards = awards;
			_userManager = userManager;
		}

		// Allows for a query based call to this page for Users/List
		[FromQuery]
		public string Name { get; set; }

		[FromRoute]
		public string UserName { get; set; }

		public UserProfileModel Profile { get; set; } = new UserProfileModel();

		public async Task<IActionResult> OnGet()
		{
			if (string.IsNullOrWhiteSpace(UserName) && string.IsNullOrWhiteSpace(Name))
			{
				return RedirectToPage("/Users/List");
			}

			if (string.IsNullOrWhiteSpace(UserName))
			{
				UserName = Name;
			}

			Profile = await _userManager.GetUserProfile(UserName, includeHidden: false);
			if (Profile == null)
			{
				return NotFound();
			}

			if (!string.IsNullOrWhiteSpace(Profile.Signature))
			{
				Profile.Signature = RenderPost(Profile.Signature, true, false);
			}

			Profile.Awards = await _awards.ForUser(Profile.Id);
			return Page();
		}
	}
}
