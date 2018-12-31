using System.Net.Mime;
using System.Threading.Tasks;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

using TASVideos.Models;
using TASVideos.Tasks;

namespace TASVideos.Pages.Submissions
{
	[AllowAnonymous]
	public class ViewModel : BasePageModel
	{
		private readonly SubmissionTasks _submissionTasks;

		public ViewModel(
			SubmissionTasks submissionTasks,
			UserTasks userTasks)
			: base(userTasks)
		{
			_submissionTasks = submissionTasks;
		}

		[FromRoute]
		public int Id { get; set; }

		public SubmissionDisplayModel Submission { get; set; } = new SubmissionDisplayModel();

		public async Task<IActionResult> OnGet()
		{
			var submission = await _submissionTasks.GetSubmission(Id, User.Identity.Name);
			if (submission == null)
			{
				return NotFound();
			}

			return Page();
		}

		public async Task<IActionResult> OnGetDownload()
		{
			var submissionFile = await _submissionTasks.GetSubmissionFile(Id);
			if (submissionFile.Length > 0)
			{
				return File(submissionFile, MediaTypeNames.Application.Octet, $"submission{Id}.zip");
			}

			return BadRequest();
		}
	}
}
