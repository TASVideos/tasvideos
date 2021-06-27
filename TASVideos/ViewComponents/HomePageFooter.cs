﻿using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TASVideos.Core.Services;
using TASVideos.Data;
using TASVideos.Data.Entity;

namespace TASVideos.ViewComponents
{
	public class HomePageFooter : ViewComponent
	{
		private readonly ApplicationDbContext _db;
		private readonly IAwards _awards;
		private readonly IWikiPages _wikiPages;

		public HomePageFooter(
			ApplicationDbContext db,
			IAwards awards,
			IWikiPages wikiPages)
		{
			_db = db;
			_awards = awards;
			_wikiPages = wikiPages;
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
					EditCount = _wikiPages.Query.Count(wp => wp.CreateUserName == userName),
					MovieCount = _db.Publications
						.Count(p => p.Authors
							.Select(sa => sa.Author!.UserName)
							.Contains(userName)),
					SubmissionCount = _db.Submissions
						.Count(s => s.SubmissionAuthors
							.Select(sa => sa.Author!.UserName)
							.Contains(userName))
				})
				.SingleOrDefaultAsync();

			if (model is not null)
			{
				model.AwardsWon = (await _awards.ForUser(model.Id)).Count();
			}

			ViewData["pageData"] = pageData;
			return View(model);
		}
	}
}
