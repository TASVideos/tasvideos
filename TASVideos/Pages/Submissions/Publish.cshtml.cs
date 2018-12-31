using System.Threading.Tasks;

using Microsoft.AspNetCore.Mvc;

using TASVideos.Data.Entity;
using TASVideos.Models;
using TASVideos.Tasks;

namespace TASVideos.Pages.Submissions
{
	[RequirePermission(PermissionTo.PublishMovies)]
	public class PublishModel : BasePageModel
	{
		private readonly SubmissionTasks _submissionTasks;

		public PublishModel(
			SubmissionTasks submissionTasks,
			UserTasks userTasks)
			: base(userTasks)
		{
			_submissionTasks = submissionTasks;
		}

		[FromRoute]
		public int Id { get; set; }

		[BindProperty]
		public SubmissionPublishModel Submission { get; set; } = new SubmissionPublishModel();

		public async Task<IActionResult> OnGet()
		{
			if (!await _submissionTasks.CanPublish(Id))
			{
				return NotFound();
			}

			Submission = await _submissionTasks.GetSubmissionForPublish(Id);
			return Page();
		}

		public async Task<IActionResult> OnPost()
		{
			if (Submission.Screenshot.ContentType != "image/png"
				&& Submission.Screenshot.ContentType != "image/jpeg")
			{
				// TODO: fix name
				ModelState.AddModelError(nameof(Submission.Screenshot), "Invalid file type. Must be .png or .jpg");
			}

			if (Submission.TorrentFile.Name != "TorrentFile")
			{
				// TODO: fix name
				ModelState.AddModelError(nameof(Submission.TorrentFile), "Invalid file type. Must be a .torrent file");
			}

			if (!ModelState.IsValid)
			{
				Submission.AvailableMoviesToObsolete = await _submissionTasks.GetAvailableMoviesToObsolete(Submission.SystemId);
				return Page();
			}

			var id = await _submissionTasks.PublishSubmission(Submission);
			return Redirect($"/{id}M");
		}
	}
}
