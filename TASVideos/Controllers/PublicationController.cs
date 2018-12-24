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

		[AllowAnonymous]
		public IActionResult Index()
		{
			return RedirectToAction(nameof(List));
		}

		[AllowAnonymous]
		public async Task<IActionResult> List(string query)
		{
			var tokenLookup = await _publicationTasks.GetMovieTokenData();

			var tokens = query
				.Split(new[] { "-" }, StringSplitOptions.RemoveEmptyEntries)
				.Select(s => s.Trim(' '))
				.Select(s => s.ToLower())
				.ToList();

			var searchModel = new PublicationSearchModel
			{
				Tiers = tokenLookup.Tiers.Where(t => tokens.Contains(t)),
				SystemCodes = tokenLookup.SystemCodes.Where(s => tokens.Contains(s)),
				ShowObsoleted = tokens.Contains("obs"),
				Years = tokenLookup.Years.Where(y => tokens.Contains("y" + y)),
				Tags = tokenLookup.Tags.Where(t => tokens.Contains(t)),
				Genres = tokenLookup.Genres.Where(g => tokens.Contains(g)),
				Flags = tokenLookup.Flags.Where(f => tokens.Contains(f)),
				MovieIds = tokens
					.Where(t => t.EndsWith('m'))
					.Where(t => int.TryParse(t.Substring(0, t.Length - 1), out int unused))
					.Select(t => int.Parse(t.Substring(0, t.Length - 1)))
					.ToList(),
				Authors = tokens
					.Where(t => t.ToLower().Contains("author"))
					.Select(t => t.ToLower().Replace("author", ""))
					.Select(t => int.TryParse(t, out var temp) ? temp : (int?)null)
					.Where(t => t.HasValue)
					.Select(t => t.Value)
					.ToList()
			};

			// If no valid filter criteria, don't attempt to generate a list (else it would be all movies for what is most likely a malformed URL)
			if (searchModel.IsEmpty)
			{
				return Redirect("Movies");
			}

			var model = (await _publicationTasks
				.GetMovieList(searchModel))
				.ToList();

			var ratings = await _ratingsTasks.GetOverallRatingsForPublications(model.Select(m => m.Id));

			foreach (var rating in ratings)
			{
				model.First(m => m.Id == rating.Key).OverallRating = rating.Value;
			}

			return View(model);
		}

		[AllowAnonymous]
		public async Task<IActionResult> View(int id)
		{
			var model = await _publicationTasks.GetPublicationForDisplay(id);
			if (model == null)
			{
				return NotFound();
			}

			model.OverallRating = await _ratingsTasks.GetOverallRatingForPublication(id);

			return View(model);
		}

		[AllowAnonymous]
		public async Task<IActionResult> Download(int id)
		{
			var (fileBytes, fileName) = await _publicationTasks.GetPublicationMovieFile(id);
			if (fileBytes.Length > 0)
			{
				return File(fileBytes, MediaTypeNames.Application.Octet, $"{fileName}.zip");
			}

			return BadRequest();
		}

		[RequirePermission(PermissionTo.EditPublicationMetaData)]
		public async Task<IActionResult> Edit(int id)
		{
			var model = await _publicationTasks.GetPublicationForEdit(id, UserPermissions);

			if (model == null)
			{
				return NotFound();
			}

			return View(model);
		}

		[HttpPost, AutoValidateAntiforgeryToken]
		[RequirePermission(PermissionTo.EditPublicationMetaData)]
		public async Task<IActionResult> Edit(PublicationEditModel model)
		{
			if (!ModelState.IsValid)
			{
				model.AvailableMoviesForObsoletedBy =
					await _publicationTasks.GetAvailableMoviesForObsoletedBy(model.Id, model.SystemCode);
				model.AvailableFlags = await _publicationTasks.GetAvailableFlags(UserPermissions);
				model.AvailableTags = await _publicationTasks.GetAvailableTags();

				return View(model);
			}

			await _publicationTasks.UpdatePublication(model);
			return RedirectToAction(nameof(View), new { model.Id });
		}

		[RequirePermission(PermissionTo.SetTier)]
		public async Task<IActionResult> EditTier(int id)
		{
			var model = await _publicationTasks.GetTiersForEdit(id);
			if (model == null)
			{
				return NotFound();
			}

			return View(model);
		}

		[HttpPost, AutoValidateAntiforgeryToken]
		[RequirePermission(PermissionTo.SetTier)]
		public async Task<IActionResult> EditTier(PublicationTierEditModel model)
		{
			var result = await _publicationTasks.UpdateTier(model.Id, model.TierId);
			if (result)
			{
				return RedirectToAction(nameof(View), new { model.Id });
			}

			return NotFound();
		}

		[RequirePermission(PermissionTo.CatalogMovies)]
		public async Task<IActionResult> Catalog(int id)
		{
			var model = await _publicationTasks.Catalog(id);
			if (model == null)
			{
				return NotFound();
			}

			return View(model);
		}

		[HttpPost, AutoValidateAntiforgeryToken]
		[RequirePermission(PermissionTo.CatalogMovies)]
		public async Task<IActionResult> Catalog(PublicationCatalogModel model)
		{
			if (!ModelState.IsValid)
			{
				return View(model); // TODO: repopulate dropdowns
			}

			await _publicationTasks.UpdateCatalog(model);
			return RedirectToAction(nameof(View), new { model.Id });
		}
		
		[AllowAnonymous]
		public async Task<IActionResult> Authors()
		{
			var model = await _publicationTasks.GetPublishedAuthorList();
			return View(model);
		}

		// TODO: permission to rate movies
		[Authorize]
		public async Task<IActionResult> Rate(int id)
		{
			var user = await _userManager.GetUserAsync(User);
			var model = await _publicationTasks.GetRatingModel(user, id);
			if (model == null)
			{
				return NotFound();
			}

			return View(model);
		}

		// TODO: permission to rate movies
		[Authorize]
		[HttpPost, ValidateAntiForgeryToken]
		public async Task<IActionResult> Rate(PublicationRateModel model)
		{
			if (!ModelState.IsValid)
			{
				return View(model);
			}

			var user = await _userManager.GetUserAsync(User);
			await _publicationTasks.RatePublication(model, user);

			// TODO: implement returnUrl, can return to either publication or user ratings
			return RedirectToAction(nameof(ProfileController.Ratings), "Profile");
		}
	}
}
