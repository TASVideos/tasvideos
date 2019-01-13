using System.Linq;
using System.Net.Mime;
using System.Threading.Tasks;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

using TASVideos.Data;
using TASVideos.Models;
using TASVideos.Tasks;

namespace TASVideos.Pages.Submissions
{
	[AllowAnonymous]
	public class ViewModel : BasePageModel
	{
		private readonly ApplicationDbContext _db;
		private readonly SubmissionTasks _submissionTasks;

		public ViewModel(
			ApplicationDbContext db,
			SubmissionTasks submissionTasks,
			UserTasks userTasks)
			: base(userTasks)
		{
			_db = db;
			_submissionTasks = submissionTasks;
		}

		[FromRoute]
		public int Id { get; set; }

		public SubmissionDisplayModel Submission { get; set; } = new SubmissionDisplayModel();

		public async Task<IActionResult> OnGet()
		{
			Submission = await _submissionTasks.GetSubmission(Id, User.Identity.Name);
			if (Submission == null)
			{
				return NotFound();
			}

			return Page();
		}

		public async Task<IActionResult> OnGetDownload()
		{
			var submissionFile = await _db.Submissions
				.Where(s => s.Id == Id)
				.Select(s => s.MovieFile)
				.SingleOrDefaultAsync();

			if (submissionFile == null)
			{
				return NotFound();
			}

			return File(submissionFile, MediaTypeNames.Application.Octet, $"submission{Id}.zip");
		}
	}
}
