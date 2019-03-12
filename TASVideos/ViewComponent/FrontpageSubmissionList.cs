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
			var maxDays = ParamHelper.GetInt(pp, "maxdays");
			var maxRecords = ParamHelper.GetInt(pp, "maxrels");
			var request = new SubmissionSearchRequest
			{
				StartDate = DateTime.UtcNow.AddDays(0 - (maxDays ?? 365))
			};

			var subs = await _db.Submissions
				.FilterBy(request)
				.Take(maxRecords ?? 5)
				.PersistToSubListEntry();

			return View(subs);
		}
	}
}
