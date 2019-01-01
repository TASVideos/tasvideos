using System.Threading.Tasks;

using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

using TASVideos.Data.Entity;
using TASVideos.Models;
using TASVideos.Tasks;

namespace TASVideos.Pages.Publications
{
	[RequirePermission(PermissionTo.RateMovies)]
	public class RateModel : BasePageModel
	{
		private readonly UserManager<User> _userManager;
		private readonly PublicationTasks _publicationTasks;

		public RateModel(
			UserManager<User> userManager,
			PublicationTasks publicationTasks,
			UserTasks userTasks)
			: base(userTasks)
		{
			_userManager = userManager;
			_publicationTasks = publicationTasks;
		}

		[FromRoute]
		public int Id { get; set; }
		
		[FromQuery]
		public string ReturnUrl { get; set; }

		[BindProperty]
		public PublicationRateModel Rating { get; set; }

		public async Task<IActionResult> OnGet()
		{
			var user = await _userManager.GetUserAsync(User);
			Rating = await _publicationTasks.GetRatingModel(user, Id);
			if (Rating == null)
			{
				return NotFound();
			}

			return Page();
		}

		public async Task<IActionResult> OnPost()
		{
			if (!Rating.EntertainmentRating.HasValue && !Rating.TechRating.HasValue)
			{
				ModelState.AddModelError("", "At least one rating must be set");
			}

			if (!ModelState.IsValid)
			{
				return Page();
			}

			var user = await _userManager.GetUserAsync(User);
			await _publicationTasks.RatePublication(Id, Rating, user);

			if (!string.IsNullOrWhiteSpace(ReturnUrl))
			{
				return RedirectToLocal(ReturnUrl);
			}

			return RedirectToPage("/Profile/Ratings");
		}
	}
}
