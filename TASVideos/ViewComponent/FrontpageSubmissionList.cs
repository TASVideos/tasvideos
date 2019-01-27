using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;

using TASVideos.Data;
using TASVideos.Data.Entity;
using TASVideos.Extensions;
using TASVideos.Models;
using TASVideos.Tasks;

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
			var request = new SubmissionSearchRequest
			{
				Limit = 5,
				Cutoff = DateTime.UtcNow.AddDays(-365)
			};

			var subs = await _db.Submissions
				.SearchBy(request)
				.PersistToSubListEntry();

			return View(subs);
		}
	}
}
