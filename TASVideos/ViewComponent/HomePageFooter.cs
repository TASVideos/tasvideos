using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

using TASVideos.Data;
using TASVideos.Data.Entity;
using TASVideos.Models;
using TASVideos.Tasks;

namespace TASVideos.ViewComponents
{
    public class HomePageFooter : ViewComponent
    {
		private readonly ApplicationDbContext _db;
		private readonly AwardTasks _awardTasks;

		public HomePageFooter(
			ApplicationDbContext db,
			AwardTasks awardTasks)
		{
			_db = db;
			_awardTasks = awardTasks;
		}

		public async Task<IViewComponentResult> InvokeAsync(WikiPage pageData)
		{
			var userName = pageData.PageName.Replace("HomePages/", "").Split('/').First();
			var model = await _db.Users
				.Where(u => u.UserName == userName)
				.Select(user => new UserSummaryModel
				{
					Id = user.Id,
					UserName = user.UserName,
					EditCount = _db.WikiPages.Count(wp => wp.CreateUserName == userName),
					MovieCount = _db.Publications
						.Count(p => p.Authors
							.Select(sa => sa.Author.UserName)
							.Contains(userName)),
					SubmissionCount = _db.Submissions
						.Count(s => s.SubmissionAuthors
							.Select(sa => sa.Author.UserName)
							.Contains(userName))
				})
				.SingleOrDefaultAsync();

			model.AwardsWon = (await _awardTasks.GetAllAwardsForUser(model.Id)).Count();

			ViewData["pageData"] = pageData;
			return View(model);
		}
	}
}
