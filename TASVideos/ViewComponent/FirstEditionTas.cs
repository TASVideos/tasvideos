using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

using TASVideos.Data;
using TASVideos.Data.Entity;
using TASVideos.ViewComponents.Models;

namespace TASVideos.ViewComponents
{
	public class FirstEditionTas : ViewComponent
	{
		private readonly ApplicationDbContext _db;

		public FirstEditionTas(ApplicationDbContext db)
		{
			_db = db;
		}

		public async Task<IViewComponentResult> InvokeAsync(WikiPage pageData, string pp)
		{
			int before = ParamHelper.GetYear(pp, "before")
				?? DateTime.UtcNow.Year;
			int after = ParamHelper.GetYear(pp, "after")
				?? DateTime.UtcNow.AddYears(1).Year;

			var beforeYear = new DateTime(before, 1, 1);
			var afterYear = new DateTime(after, 1, 1);

			// TODO: first edition logic
			var model = await _db.Publications
				.ThatAreCurrent()
				.Where(p => p.CreateTimeStamp >= afterYear)
				.Where(p => p.CreateTimeStamp < beforeYear)
				.Select(p => new FirstEditionModel
				{
					Id = p.Id,
					Title = p.Title,
					GameId = p.Id,
					TierId = p.TierId,
					TierIconPath = p.Tier.IconPath,
					TierName = p.Tier.Name
				})
				.ToListAsync();

			return View(model);
		}
	}
}
