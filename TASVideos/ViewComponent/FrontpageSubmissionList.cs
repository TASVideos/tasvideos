using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;

using TASVideos.Data;
using TASVideos.Data.Entity;
using TASVideos.Extensions;
using TASVideos.Pages.Submissions.Models;

namespace TASVideos.ViewComponents
{
	public class FrontpageSubmissionList : ViewComponent
	{
		private readonly ApplicationDbContext _db;

		public FrontpageSubmissionList(ApplicationDbContext db)
		{
			_db = db;
		}

		public async Task<IViewComponentResult> InvokeAsync(WikiPage pageData, string pp)
		{
			// Legacy system supported a max days value, which isn't easily translated to the current filtering
			// However, we currently have it set to 365 which greatly exceeds any max number
			// And submissions are frequent enough to not worry about too stale submissions showing up on the front page
			var maxRecords = ParamHelper.GetInt(pp, "maxrels");
			var request = new SubmissionSearchRequest();

			var subs = await _db.Submissions
				.FilterBy(request)
				.Take(maxRecords ?? 5)
				.PersistToSubListEntry();

			return View(subs);
		}
	}
}
