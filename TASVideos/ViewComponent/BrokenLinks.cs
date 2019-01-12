using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

using TASVideos.Data;
using TASVideos.Data.Entity;
using TASVideos.Data.Helpers;

namespace TASVideos.ViewComponents
{
	public class BrokenLinks : ViewComponent
	{
		private readonly ApplicationDbContext _db;

		public BrokenLinks(ApplicationDbContext db)
		{
			_db = db;
		}

		public async Task<IViewComponentResult> InvokeAsync(WikiPage pageData, string pp)
		{
			var orphans = (await _db.WikiReferrals
					.Where(wr => wr.Referrer != "SandBox")
					.Where(wr => wr.Referral != "Players-List")
					.Where(wr => !_db.WikiPages.Any(wp => wp.PageName == wr.Referral))
					.Where(wr => !wr.Referral.StartsWith("Subs-"))
					.Where(wr => !wr.Referral.StartsWith("Movies-"))
					.Where(wr => !wr.Referral.StartsWith("/forum"))
					.Where(wr => !wr.Referral.StartsWith("/userfiles"))
					.Where(wr => !string.IsNullOrWhiteSpace(wr.Referral))
					.Where(wr => wr.Referral != "FrontPage")
					.ToListAsync())
				.Where(wr => !SubmissionHelper.IsSubmissionLink(wr.Referral).HasValue)
				.Where(wr => !SubmissionHelper.IsPublicationLink(wr.Referral).HasValue)
				.Where(wr => !SubmissionHelper.IsGamePageLink(wr.Referral).HasValue);

			return View(orphans);
		}
	}
}