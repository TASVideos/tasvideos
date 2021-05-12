using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TASVideos.Data;
using TASVideos.RazorPages.Extensions;
using TASVideos.WikiEngine;

namespace TASVideos.RazorPages.ViewComponents
{
	[WikiModule(WikiModules.GameName)]
	public class GameName : ViewComponent
	{
		private readonly ApplicationDbContext _db;

		public GameName(ApplicationDbContext db)
		{
			_db = db;
		}

		public async Task<IViewComponentResult> InvokeAsync()
		{
			var path = HttpContext.Request.Path.ToString().Trim('/');

			var gameList = new List<GameNameModel>();
			if (path.IsSystemGameResourcePath())
			{
				var system = await _db.GameSystems
					.SingleOrDefaultAsync(s => s.Code == path.SystemGameResourcePath());
				if (system is not null)
				{
					gameList.Add(new GameNameModel { System = system.DisplayName });
				}
			}
			else
			{
				gameList = await _db.Games
					.Where(g => g.GameResourcesPage == path)
					.Select(g => new GameNameModel
					{
						GameId = g.Id,
						DisplayName = g.DisplayName
					})
					.ToListAsync();
			}

			return View(gameList);
		}
	}
}
