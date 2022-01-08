using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TASVideos.Core.Services.ExternalMediaPublisher;
using TASVideos.Data;
using TASVideos.Data.Entity;
using TASVideos.WikiEngine;

namespace TASVideos.ViewComponents
{
	[WikiModule(WikiModules.MediaPosts)]
	public class MediaPosts : ViewComponent
	{
		private readonly ApplicationDbContext _db;

		public MediaPosts(ApplicationDbContext db)
		{
			_db = db;
		}

		public async Task<IViewComponentResult> InvokeAsync(int? days, int? limit)
		{
			var startDate = DateTime.UtcNow.AddDays(-(days ?? 7));
			var model = await GetPosts(startDate, limit ?? 50);

			return View(model);
		}

		public async Task<IEnumerable<MediaPost>> GetPosts(DateTime startDate, int limit)
		{
			var canSeeRestricted = UserClaimsPrincipal.Has(PermissionTo.SeeRestrictedForums);

			return await _db.MediaPosts
				.Since(startDate)
				.Where(m => canSeeRestricted || m.Type != PostType.Critical.ToString())
				.Where(m => canSeeRestricted || m.Type != PostType.Administrative.ToString())
				.Where(m => canSeeRestricted || m.Type != PostType.Log.ToString())
				.ByMostRecent()
				.Take(limit)
				.ToListAsync();
		}
	}
}
