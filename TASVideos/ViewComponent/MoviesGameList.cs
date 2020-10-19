using System.Linq;
using System.Threading.Tasks;

using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TASVideos.Data;
using TASVideos.Data.Entity;
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

		public async Task<IViewComponentResult> InvokeAsync(WikiPage pageData, string pp)
		{
			var model = new MoviesGameListModel
			{
				SystemId = ParamHelper.GetInt(pp, "system")
			};


			if (model.SystemId.HasValue)
			{
				var system = await _db.GameSystems.SingleOrDefaultAsync(s => s.Id == model.SystemId);

				if (system != null)
				{
					model.SystemCode = system.Code;
					model.Games = await _db.Games
						.ForSystem(model.SystemId.Value)
						.Select(g => new MoviesGameListModel.GameEntry
						{
							Id = g.Id,
							Name = g.DisplayName,
							PublicationIds = g.Publications
								.Where(p => p.ObsoletedById == null)
								.Select(p => p.Id)
								.ToList()
						})
						.ToListAsync();
				}
			}

			return View(model);
		}
	}
}