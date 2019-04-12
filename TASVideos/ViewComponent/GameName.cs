using System.Threading.Tasks;

using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

using TASVideos.Data;
using TASVideos.Data.Entity;
using TASVideos.Extensions;
using TASVideos.ViewComponents.Models;

namespace TASVideos.ViewComponents
{
	public class GameName : ViewComponent
	{
		private readonly ApplicationDbContext _db;

		public GameName(ApplicationDbContext db)
		{
			_db = db;
		}

		public async Task<IViewComponentResult> InvokeAsync(WikiPage pageData, string pp)
		{
			var path = HttpContext.Request.Path.ToString().Trim('/');

			var model = new GameNameModel();
			if (path.IsSystemGameResourcePath())
			{
				var system = await _db.GameSystems
					.SingleOrDefaultAsync(s => s.Code == path.SystemGameResourcePath());
				if (system != null)
				{
					model.System = system.DisplayName;
				}
			}
			else
			{
				var game = await _db.Games
					.SingleOrDefaultAsync(g => g.GameResourcesPage == path);
				if (game != null)
				{
					model.GameId = game.Id;
					model.DisplayName = game.DisplayName;
				}
			}

			return View(model);
		}
	}
}
