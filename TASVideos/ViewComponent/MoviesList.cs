using System.Linq;
using System.Threading.Tasks;

using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

using TASVideos.Data;
using TASVideos.Data.Entity;
using TASVideos.Data.Entity.Game;

namespace TASVideos.ViewComponents
{
	public class MoviesList: ViewComponent
	{
		private readonly ApplicationDbContext _db;

		public MoviesList(ApplicationDbContext db)
		{
			_db = db;
		}

		public async Task<IViewComponentResult> InvokeAsync(WikiPage pageData, string pp)
		{
			var systemCode = ParamHelper.GetValueFor(pp, "platform");

			bool isAll = string.IsNullOrWhiteSpace(systemCode);
			var model = new MoviesListModel { SystemCode = systemCode };
			GameSystem? system = null;

			if (!isAll)
			{
				system = await _db.GameSystems.SingleOrDefaultAsync(s => s.Code == systemCode);
				if (system == null)
				{
					return View(model);
				}
			}

			model.SystemName = system?.DisplayName ?? "ALL";
			model.Movies = await _db.Publications
				.Where(p => isAll || p.System!.Code == systemCode)
				.Select(p => new MoviesListModel.MovieEntry
				{
					Id = p.Id,
					IsObsolete = p.ObsoletedById.HasValue,
					GameName = p.Game!.DisplayName
				})
				.ToListAsync();

			return View(model);
		}
	}
}
