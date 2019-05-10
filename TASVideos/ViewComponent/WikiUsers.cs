using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using Microsoft.AspNetCore.Mvc;

using TASVideos.Data;
using TASVideos.Data.Entity;

namespace TASVideos.ViewComponents
{
	public class WikiUsers : ViewComponent
	{
		private readonly ApplicationDbContext _db;

		public WikiUsers(ApplicationDbContext db)
		{
			_db = db;
		}

		public async Task<IViewComponentResult> InvokeAsync(WikiPage pageData, string pp)
		{
			ParamHelper.GetValueFor(pp, "role");

			var model = new List<WikiUserEntry>();
			return View(model);
		}
	}
}
