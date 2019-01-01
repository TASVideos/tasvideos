using System;
using System.Linq;
using System.Net.Mime;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using TASVideos.Data.Entity;
using TASVideos.Filter;
using TASVideos.Models;
using TASVideos.Tasks;

namespace TASVideos.Controllers
{
	public class PublicationController : BaseController
	{
		private readonly UserManager<User> _userManager;
		private readonly PublicationTasks _publicationTasks;
		private readonly RatingsTasks _ratingsTasks;

		public PublicationController(
			UserManager<User> userManager,
			PublicationTasks publicationTasks,
			RatingsTasks ratingTasks,
			UserTasks userTasks)
			: base(userTasks)
		{
			_userManager = userManager;
			_publicationTasks = publicationTasks;
			_ratingsTasks = ratingTasks;
		}
		
		[RequirePermission(PermissionTo.RateMovies)]
		public async Task<IActionResult> Rate(int id, string returnUrl = null)
		{
			var user = await _userManager.GetUserAsync(User);
			var model = await _publicationTasks.GetRatingModel(user, id);
			if (model == null)
			{
				return NotFound();
			}

			return View(model);
		}

		[RequirePermission(PermissionTo.RateMovies)]
		[HttpPost, ValidateAntiForgeryToken]
		public async Task<IActionResult> Rate(PublicationRateModel model, string returnUrl = null)
		{
			if (!model.EntertainmentRating.HasValue && !model.TechRating.HasValue)
			{
				ModelState.AddModelError("", "At least one rating must be set");
			}

			if (!ModelState.IsValid)
			{
				return View(model);
			}

			var user = await _userManager.GetUserAsync(User);
			await _publicationTasks.RatePublication(model, user);

			if (!string.IsNullOrWhiteSpace(returnUrl))
			{
				return RedirectToLocal(returnUrl);
			}

			return RedirectToPage("/Profile/Ratings");
		}
	}
}
