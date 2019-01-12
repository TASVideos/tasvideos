using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

using TASVideos.Data;
using TASVideos.Data.Entity;
using TASVideos.Services.ExternalMediaPublisher;

namespace TASVideos.ViewComponents
{
    public class MediaPosts : ViewComponent
    {
		private readonly ApplicationDbContext _db;

		public MediaPosts(ApplicationDbContext db)
		{
			_db = db;
		}

		public async Task<IViewComponentResult> InvokeAsync(WikiPage pageData, string pp)
		{
			int days = ParamHelper.GetInt(pp, "days") ?? 7;
			var startDate = DateTime.Now.AddDays(-days);
			var limit = ParamHelper.GetInt(pp, "limit") ?? 50;
			var model = await GetPosts(startDate, limit);

			return View(model);
		}

		public async Task<IEnumerable<MediaPost>> GetPosts(DateTime startDate, int limit)
		{
			return await _db.MediaPosts
				.Since(startDate)
				.Where(m => m.Type != PostType.Critical.ToString()) // TODO: Permission check to see these
				.Where(m => m.Type != PostType.Administrative.ToString()) // TODO: Permission check to see these
				.Where(m => m.Type != PostType.Log.ToString()) // TODO: Permission check to see these (and/or a parameter)
				.ByMostRecent()
				.Take(limit)
				.ToListAsync();
		}
	}
}
