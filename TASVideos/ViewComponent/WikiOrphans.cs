using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

using TASVideos.Data;
using TASVideos.Data.Entity;
using TASVideos.Models;

namespace TASVideos.ViewComponents
{
	public class WikiOrphans : ViewComponent
	{
		private readonly ApplicationDbContext _db;

		public WikiOrphans(ApplicationDbContext db)
		{
			_db = db;
		}

		public async Task<IViewComponentResult> InvokeAsync(WikiPage pageData, string pp)
		{
			var orphans = await _db.WikiPages
				.ThatAreNotDeleted()
				.ThatAreCurrentRevisions()
				.Where(wp => wp.PageName != "MediaPosts") // Linked by the navbar
				.Where(wp => !_db.WikiReferrals.Any(wr => wr.Referral == wp.PageName))
				.Where(wp => !wp.PageName.StartsWith("System/")
					&& !wp.PageName.StartsWith("InternalSystem")) // These by design aren't orphans they are directly used in the system
				.Where(wp => !wp.PageName.Contains("/")) // Subpages are linked by default by the parents, so we know they are not orphans
				.Select(wp => new WikiOrphanModel
				{
					PageName = wp.PageName,
					LastUpdateTimeStamp = wp.LastUpdateTimeStamp,
					LastUpdateUserName = wp.LastUpdateUserName ?? wp.CreateUserName
				})
				.ToListAsync();

			return View(orphans);
		}
	}
}
