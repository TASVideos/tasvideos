using System.Linq;
using System.Threading.Tasks;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

using TASVideos.Models;
using TASVideos.Tasks;

namespace TASVideos.Pages.Submissions
{
	[AllowAnonymous]
	public class IndexModel : BasePageModel
	{
		private readonly SubmissionTasks _submissionTasks;

		public IndexModel(
			SubmissionTasks submissionTasks,
			UserTasks userTasks)
			: base(userTasks)
		{
			_submissionTasks = submissionTasks;
		}

		[FromQuery]
		public SubmissionSearchRequest Search { get; set; } = new SubmissionSearchRequest();

		public SubmissionListModel Submissions { get; set; } = new SubmissionListModel();

		public async Task OnGet()
		{
			// Defaults
			if (!Search.StatusFilter.Any())
			{
				Search.StatusFilter = !string.IsNullOrWhiteSpace(Search.User)
					? SubmissionSearchRequest.All
					: SubmissionSearchRequest.Default;

				Submissions = await _submissionTasks.GetSubmissionList(Search);
			}
		}
	}
}
