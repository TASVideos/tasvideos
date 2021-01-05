using System.Linq;
using System.Threading.Tasks;

using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TASVideos.Data;
using TASVideos.Data.Entity.Game;

namespace TASVideos.ViewComponents
{
	public class MoviesGameList : ViewComponent
	{
		private readonly ApplicationDbContext _db;

		public MoviesGameList(ApplicationDbContext db)
		{
			_db = db;
		}

		public async Task<IViewComponentResult> InvokeAsync(string pp)
		{
			var systemId = ParamHelper.GetInt(pp, "system");
			var system = await _db.GameSystems.SingleOrDefaultAsync(s => s.Id == systemId);
			if (systemId is null || system is null)
			{
				return View(new MoviesGameListModel
				{
					SystemId = ParamHelper.GetInt(pp, "system")
				});
			}

			var model = new MoviesGameListModel
			{
				SystemId = ParamHelper.GetInt(pp, "system"),
				SystemCode = system.Code,
				Games = await _db.Games
					.ForSystem(systemId.Value)
					.Select(g => new MoviesGameListModel.GameEntry
					{
						Id = g.Id,
						Name = g.DisplayName,
						PublicationIds = g.Publications
							.Where(p => p.ObsoletedById == null)
							.Select(p => p.Id)
							.ToList()
					})
					.ToListAsync()
			};

			return View(model);
		}
	}
}
