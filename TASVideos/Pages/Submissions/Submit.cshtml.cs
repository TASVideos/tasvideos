using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

using TASVideos.Data.Entity;
using TASVideos.Data.Helpers;
using TASVideos.Models;
using TASVideos.Services;
using TASVideos.Services.ExternalMediaPublisher;
using TASVideos.Tasks;

namespace TASVideos.Pages.Submissions
{
	[RequirePermission(PermissionTo.SubmitMovies)]
	public class SubmitModel : BasePageModel
	{
		private readonly IWikiPages _wikiPages;
		private readonly ExternalMediaPublisher _publisher;
		private readonly SubmissionTasks _submissionTasks;
		
		public SubmitModel(
			ExternalMediaPublisher publisher,
			IWikiPages wikiPages,
			SubmissionTasks submissionTasks,
			UserTasks userTasks)
			: base(userTasks)
		{
			_publisher = publisher;
			_wikiPages = wikiPages;
			_submissionTasks = submissionTasks;
		}

		[BindProperty]
		public SubmissionCreateModel Create { get; set; } = new SubmissionCreateModel();

		public void OnGet()
		{
			Create = new SubmissionCreateModel
			{
				GameVersionOptions = SubmissionHelper.GameVersionOptions,
				Authors = new List<string> { User.Identity.Name }
			};
		}

		public async Task<IActionResult> OnPost()
		{
			Create.Authors = Create.Authors
				.Where(a => !string.IsNullOrWhiteSpace(a))
				.ToList();

			if (!Create.Authors.Any())
			{
				ModelState.AddModelError(
					nameof(SubmissionCreateModel.Authors),
					"A submission must have at least one author"); // TODO: need to use the AtLeastOne attribute error message since it will be localized
			}

			if (!Create.MovieFile.FileName.EndsWith(".zip")
			|| Create.MovieFile.ContentType != "application/x-zip-compressed")
			{
				ModelState.AddModelError(nameof(SubmissionCreateModel.MovieFile), "Not a valid .zip file");
			}

			if (Create.MovieFile.Length > 150 * 1024)
			{
				ModelState.AddModelError(nameof(SubmissionCreateModel.MovieFile), ".zip is too big, are you sure this is a valid movie file?");
			}

			foreach (var author in Create.Authors)
			{
				if (!await UserTasks.CheckUserNameExists(author))
				{
					ModelState.AddModelError(nameof(SubmissionCreateModel.Authors), $"Could not find user: {author}");
				}
			}

			if (ModelState.IsValid)
			{
				var result = await _submissionTasks.SubmitMovie(Create, User.Identity.Name);
				if (result.Success)
				{
					var title = await _submissionTasks.GetTitle(result.Id); // TODO: we could return the submission and not have to take this extra query hit
					_publisher.AnnounceSubmission(title, $"{BaseUrl}/{result.Id}S");

					return Redirect($"/{result.Id}S");
				}

				foreach (var error in result.Errors)
				{
					ModelState.AddModelError("", error);
				}
			}

			Create.GameVersionOptions = SubmissionHelper.GameVersionOptions;
			return Page();
		}

		public IActionResult OnGetPrefillText()
		{
			var page = _wikiPages.Page("System/SubmissionDefaultMessage");
			return new JsonResult(new { text = page.Markup });
		}
	}
}
