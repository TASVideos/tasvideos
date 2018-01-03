using System.Collections.Generic;
using System.Linq;
using System.Net.Mime;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using TASVideos.Data.Entity;
using TASVideos.Filter;
using TASVideos.Models;
using TASVideos.Tasks;

namespace TASVideos.Controllers
{
	public class SubmissionController : BaseController
	{
		private readonly UserTasks _userTasks;
		private readonly WikiTasks _wikiTasks;
		private readonly SubmissionTasks _submissionTasks;

		public SubmissionController(
			UserTasks userTasks,
			WikiTasks wikiTasks,
			SubmissionTasks submissionTasks)
			: base(userTasks)
		{
			_userTasks = userTasks;
			_wikiTasks = wikiTasks;
			_submissionTasks = submissionTasks;
		}

		// Submisison List
		[AllowAnonymous]
		public IActionResult Index()
		{
			return View();
		}

		[RequirePermission(PermissionTo.SubmitMovies)]
		public IActionResult Submit()
		{
			var model = new SubmissionCreateViewModel
			{
				GameVersionOptions = GameVersionOptions,
				Authors = new List<string> { User.Identity.Name }
			};

			return View(model);
		}

		[HttpPost]
		[RequirePermission(PermissionTo.SubmitMovies)]
		public async Task<IActionResult> Submit(SubmissionCreateViewModel model)
		{
			model.Authors = model.Authors
				.Where(a => !string.IsNullOrWhiteSpace(a))
				.ToList();

			if (!model.Authors.Any())
			{
				ModelState.AddModelError(nameof(SubmissionCreateViewModel.Authors), "A submission must have at least one author"); // TODO: need to use the ATLeastOne attribute error message since it will be localized
			}

			foreach (var author in model.Authors)
			{
				if (!await _userTasks.CheckUserNameExists(author))
				{
					ModelState.AddModelError(nameof(SubmissionCreateViewModel.Authors), $"Could not find user: {author}");
				}
			}

			if (!model.MovieFile.FileName.EndsWith(".zip")
			|| model.MovieFile.ContentType != "application/x-zip-compressed")
			{
				ModelState.AddModelError(nameof(SubmissionCreateViewModel.MovieFile), "Not a valid .zip file");
			}

			if (model.MovieFile.Length > 150 * 1024)
			{
				ModelState.AddModelError(nameof(SubmissionCreateViewModel.MovieFile), ".zip is too big, are you sure this is a valid movie file?");
			}

			if (ModelState.IsValid)
			{
				var result = await _submissionTasks.SubmitMovie(model, User.Identity.Name);
				if (result.Success)
				{
					return Redirect($"/{result.Id}S");
				}

				foreach (var error in result.Errors)
				{
					ModelState.AddModelError("", error);
				}
			}

			model.GameVersionOptions = GameVersionOptions;
			return View(model);
		}

		[RequirePermission(PermissionTo.SubmitMovies)]
		public async Task<IActionResult> PrefillText()
		{
			var page = await _wikiTasks.GetPage("System/SubmissionDefaultMessage");
			return Json(new {text = page.Markup});
		}

		[AllowAnonymous]
		public async Task<IActionResult> View(int id)
		{
			var submission = await _submissionTasks.GetSubmission(id);
			if (submission == null)
			{
				return NotFound();
			}

			return View(submission);
		}

		[AllowAnonymous]
		public async Task<IActionResult> Download(int id)
		{
			var submissionFile = await _submissionTasks.GetSubmissionFile(id);
			if (submissionFile.Length > 0)
			{
				return File(submissionFile, MediaTypeNames.Application.Octet, $"submission{id}.zip");
			}

			return BadRequest();
		}

		[RequirePermission(PermissionTo.EditSubmissions)]
		public async Task<IActionResult> Edit(int id)
		{
			var submission = await _submissionTasks.GetSubmissionForEdit(id);

			if (submission == null)
			{
				return NotFound();
			}

			submission.GameVersionOptions = GameVersionOptions;
			return View(submission);
		}

		[HttpPost]
		[RequirePermission(PermissionTo.EditSubmissions)]
		public async Task<IActionResult> Edit(SubmissionEditModel model)
		{
			if (ModelState.IsValid)
			{
				await _submissionTasks.UpdateSubmission(model);
				return Redirect($"/{model.Id}S");
			}

			model.GameVersionOptions = GameVersionOptions;
			return View(model);
		}

		private static readonly SelectListItem[] GameVersionOptions =
		{
			new SelectListItem {Text = "unknown", Value = "unknown"},
			new SelectListItem {Text = "unknown v1.0", Value = "unknown v1.0"},
			new SelectListItem {Text = "unknown v1.1", Value = "unknown v1.1"},
			new SelectListItem {Text = "unknown,r0", Value = "unknown,r0"},
			new SelectListItem {Text = "unknown,r1", Value = "unknown,r1"},
			new SelectListItem {Text = "unknown,r2", Value = "unknown,r2"},
			new SelectListItem {Text = "unknown PRG0", Value = "unknown PRG0"},
			new SelectListItem {Text = "unknown PRG1", Value = "unknown PRG1"},
			new SelectListItem {Text = "unknown PRG2", Value = "unknown PRG2"},
			new SelectListItem {Text = "any", Value = "any"},
			new SelectListItem {Text = "any v1.0", Value = "any v1.0"},
			new SelectListItem {Text = "any v1.1", Value = "any v1.1"},
			new SelectListItem {Text = "any,r0", Value = "any,r0"},
			new SelectListItem {Text = "any,r1", Value = "any,r1"},
			new SelectListItem {Text = "any,r2", Value = "any,r2"},
			new SelectListItem {Text = "any PRG0", Value = "any PRG0"},
			new SelectListItem {Text = "any PRG1", Value = "any PRG1"},
			new SelectListItem {Text = "any PRG2", Value = "any PRG2"},
			new SelectListItem {Text = "Europe", Value = "Europe"},
			new SelectListItem {Text = "Europe v1.0", Value = "Europe v1.0"},
			new SelectListItem {Text = "Europe v1.1", Value = "Europe v1.1"},
			new SelectListItem {Text = "Europe,r0", Value = "Europe,r0"},
			new SelectListItem {Text = "Europe,r1", Value = "Europe,r1"},
			new SelectListItem {Text = "Europe,r2", Value = "Europe,r2"},
			new SelectListItem {Text = "Europe PRG0", Value = "Europe PRG0"},
			new SelectListItem {Text = "Europe PRG1", Value = "Europe PRG1"},
			new SelectListItem {Text = "Europe PRG2", Value = "Europe PRG2"},
			new SelectListItem {Text = "FDS", Value = "FDS"},
			new SelectListItem {Text = "FDS v1.0", Value = "FDS v1.0"},
			new SelectListItem {Text = "FDS v1.1", Value = "FDS v1.1"},
			new SelectListItem {Text = "FDS,r0", Value = "FDS,r0"},
			new SelectListItem {Text = "FDS,r1", Value = "FDS,r1"},
			new SelectListItem {Text = "FDS,r2", Value = "FDS,r2"},
			new SelectListItem {Text = "FDS PRG0", Value = "FDS PRG0"},
			new SelectListItem {Text = "FDS PRG1", Value = "FDS PRG1"},
			new SelectListItem {Text = "FDS PRG2", Value = "FDS PRG2"},
			new SelectListItem {Text = "JPN", Value = "JPN"},
			new SelectListItem {Text = "JPN v1.0", Value = "JPN v1.0"},
			new SelectListItem {Text = "JPN v1.1", Value = "JPN v1.1"},
			new SelectListItem {Text = "JPN,r0", Value = "JPN,r0"},
			new SelectListItem {Text = "JPN,r1", Value = "JPN,r1"},
			new SelectListItem {Text = "JPN,r2", Value = "JPN,r2"},
			new SelectListItem {Text = "JPN PRG0", Value = "JPN PRG0"},
			new SelectListItem {Text = "JPN PRG1", Value = "JPN PRG1"},
			new SelectListItem {Text = "JPN PRG2", Value = "JPN PRG2"},
			new SelectListItem {Text = "JPN/USA", Value = "JPN/USA"},
			new SelectListItem {Text = "JPN/USA v1.0", Value = "JPN/USA v1.0"},
			new SelectListItem {Text = "JPN/USA v1.1", Value = "JPN/USA v1.1"},
			new SelectListItem {Text = "JPN/USA,r0", Value = "JPN/USA,r0"},
			new SelectListItem {Text = "JPN/USA,r1", Value = "JPN/USA,r1"},
			new SelectListItem {Text = "JPN/USA,r2", Value = "JPN/USA,r2"},
			new SelectListItem {Text = "JPN/USA PRG0", Value = "JPN/USA PRG0"},
			new SelectListItem {Text = "JPN/USA PRG1", Value = "JPN/USA PRG1"},
			new SelectListItem {Text = "JPN/USA PRG2", Value = "JPN/USA PRG2"},
			new SelectListItem {Text = "USA", Value = "USA"},
			new SelectListItem {Text = "USA v1.0", Value = "USA v1.0"},
			new SelectListItem {Text = "USA v1.1", Value = "USA v1.1"},
			new SelectListItem {Text = "USA,r0", Value = "USA,r0"},
			new SelectListItem {Text = "USA,r1", Value = "USA,r1"},
			new SelectListItem {Text = "USA,r2", Value = "USA,r2"},
			new SelectListItem {Text = "USA PRG0", Value = "USA PRG0"},
			new SelectListItem {Text = "USA PRG1", Value = "USA PRG1"},
			new SelectListItem {Text = "USA PRG2", Value = "USA PRG2"},
			new SelectListItem {Text = "USA/Europe", Value = "USA/Europe"},
			new SelectListItem {Text = "USA/Europe v1.0", Value = "USA/Europe v1.0"},
			new SelectListItem {Text = "USA/Europe v1.1", Value = "USA/Europe v1.1"},
			new SelectListItem {Text = "USA/Europe,r0", Value = "USA/Europe,r0"},
			new SelectListItem {Text = "USA/Europe,r1", Value = "USA/Europe,r1"},
			new SelectListItem {Text = "USA/Europe,r2", Value = "USA/Europe,r2"},
			new SelectListItem {Text = "USA/Europe PRG0", Value = "USA/Europe PRG0"},
			new SelectListItem {Text = "USA/Europe PRG1", Value = "USA/Europe PRG1"},
			new SelectListItem {Text = "USA/Europe PRG2", Value = "USA/Europe PRG2"},
		};
	}
}
