using System;
using System.Linq;
using System.Net.Mime;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TASVideos.Filter;
using TASVideos.Models;
using TASVideos.Tasks;
using TASVideos.Data.Entity;

namespace TASVideos.Controllers
{
	public class PublicationController : BaseController
	{
		private readonly PublicationTasks _publicationTasks;

		public PublicationController(
			PublicationTasks publicationTasks,
			UserTasks userTasks)
			: base(userTasks)
		{
			_publicationTasks = publicationTasks;
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
				Years = tokenLookup.Years.Where(y => tokens.Contains("Y" + y)),
				Tags = tokenLookup.Tags.Where(t => tokens.Contains(t))
			};

			var model = await _publicationTasks.GetMovieList(searchModel);
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

			return View(model);
		}

		[AllowAnonymous]
		public async Task<IActionResult> Download(int id)
		{
			(var fileBytes, var fileName) = await _publicationTasks.GetPublicationMovieFile(id);
			if (fileBytes.Length > 0)
			{
				return File(fileBytes, MediaTypeNames.Application.Octet, $"{fileName}.zip");
			}

			return BadRequest();
		}

		[RequirePermission(PermissionTo.EditPublicationMetaData)]
		public async Task<IActionResult> Edit(int id)
		{
			var model = await _publicationTasks.GetPublicationForEdit(id);

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
				return View(model);
			}

			await _publicationTasks.UpdatePublication(model);
			return RedirectToAction(nameof(View), new { model.Id });
		}
	}
}
