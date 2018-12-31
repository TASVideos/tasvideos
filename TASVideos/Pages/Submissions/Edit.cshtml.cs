using System.Linq;
using System.Threading.Tasks;

using Microsoft.AspNetCore.Mvc;

using TASVideos.Data.Entity;
using TASVideos.Data.Helpers;
using TASVideos.Models;
using TASVideos.Tasks;

namespace TASVideos.Pages.Submissions
{
	[RequirePermission(true, PermissionTo.SubmitMovies, PermissionTo.EditSubmissions)]
	public class EditModel : BasePageModel
	{
		private readonly SubmissionTasks _submissionTasks;

		public EditModel(
			SubmissionTasks submissionTasks,
			UserTasks userTasks)
			: base(userTasks)
		{
			_submissionTasks = submissionTasks;
		}

		[FromRoute]
		public int Id { get; set; }

		[BindProperty]
		public SubmissionEditModel Submission { get; set; } = new SubmissionEditModel();

		public async Task<IActionResult> OnGet()
		{
			Submission = await _submissionTasks.GetSubmissionForEdit(Id);
			
			if (Submission == null)
			{
				return NotFound();
			}

			Submission.AvailableStatuses = SubmissionHelper.AvailableStatuses(
				Submission.Status,
				UserPermissions,
				Submission.CreateTimestamp,
				Submission.Submitter == User.Identity.Name || Submission.Authors.Contains(User.Identity.Name),
				Submission.Judge == User.Identity.Name);

			// If user can not edit submissions then they must be an author or the original submitter
			if (!UserHas(PermissionTo.EditSubmissions))
			{
				if (Submission.Submitter != User.Identity.Name
					&& !Submission.Authors.Contains(User.Identity.Name))
				{
					return AccessDenied();
				}
			}

			Submission.GameVersionOptions = SubmissionHelper.GameVersionOptions;
			return Page();
		}

		public async Task<IActionResult> OnPost()
		{
			if (UserHas(PermissionTo.ReplaceSubmissionMovieFile) && Submission.MovieFile != null)
			{
				if (!Submission.MovieFile.FileName.EndsWith(".zip")
					|| Submission.MovieFile.ContentType != "application/x-zip-compressed")
				{
					ModelState.AddModelError(nameof(SubmissionCreateModel.MovieFile), "Not a valid .zip file");
				}

				if (Submission.MovieFile.Length > 150 * 1024)
				{
					ModelState.AddModelError(
						nameof(SubmissionCreateModel.MovieFile),
						".zip is too big, are you sure this is a valid movie file?");
				}
			}
			else if (!UserHas(PermissionTo.ReplaceSubmissionMovieFile))
			{
				Submission.MovieFile = null;
			}

			// TODO: this is bad, an author can null out these values,
			// but if we treat null as no choice, then we have no way to unset these values
			if (!UserHas(PermissionTo.JudgeSubmissions))
			{
				Submission.TierId = null;
			}

			var subInfo = await _submissionTasks.GetStatusVerificationValues(Id, User.Identity.Name);
			var availableStatus = SubmissionHelper.AvailableStatuses(
				subInfo.CurrentStatus,
				UserPermissions,
				subInfo.CreateDate,
				subInfo.UserIsAuthorOrSubmitter,
				subInfo.UserIsJudge)
				.ToList();

			if (!Submission.TierId.HasValue
				&& (Submission.Status == SubmissionStatus.Accepted || Submission.Status == SubmissionStatus.PublicationUnderway))
			{
				ModelState.AddModelError(nameof(Submission.TierId), "A submission can not be accepted without a Tier");
			}

			if (ModelState.IsValid)
			{
				if (!availableStatus.Contains(Submission.Status))
				{
					ModelState.AddModelError(nameof(Submission.Status), $"Invalid status: {Submission.Status}");
				}

				// If user can not edit submissions then they must be an author or the original submitter
				if (!UserHas(PermissionTo.EditSubmissions))
				{
					if (!subInfo.UserIsAuthorOrSubmitter)
					{
						return AccessDenied();
					}
				}

				var result = await _submissionTasks.UpdateSubmission(Id, Submission, User.Identity.Name);
				if (result.Success)
				{
					return Redirect($"/{Id}S");
				}

				foreach (var error in result.Errors)
				{
					ModelState.AddModelError("", error);
				}
			}

			Submission.AvailableTiers = await _submissionTasks.GetAvailableTiers();
			Submission.GameVersionOptions = SubmissionHelper.GameVersionOptions;
			Submission.AvailableStatuses = availableStatus;
			return Page();
		}
	}
}
