using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using TASVideos.Data.Entity;
using TASVideos.Models;
using TASVideos.Tasks;

namespace TASVideos.ViewComponents
{
    public class FrontpageSubmissionList : ViewComponent
    {
		private readonly SubmissionTasks _submissionTasks;

		public FrontpageSubmissionList(SubmissionTasks submissionTasks)
		{
			_submissionTasks = submissionTasks;
		}

		public async Task<IViewComponentResult> InvokeAsync(WikiPage pageData, string pp)
		{
			var model = new SubmissionSearchRequest
			{
				Limit = 5,
				Cutoff = DateTime.UtcNow.AddDays(-365)
			};

			var subs = await _submissionTasks.GetSubmissionList(model);

			return View(subs);
		}
	}
}
