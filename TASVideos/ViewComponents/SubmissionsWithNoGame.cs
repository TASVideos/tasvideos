using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TASVideos.Data;
using TASVideos.WikiEngine;

namespace TASVideos.ViewComponents
{
	[WikiModule(WikiModules.NoGameName)]
	public class SubmissionsWithNoGames : ViewComponent
	{
		private readonly ApplicationDbContext _db;

		public SubmissionsWithNoGames(ApplicationDbContext db)
		{
			_db = db;
		}

		public async Task<IViewComponentResult> InvokeAsync()
		{
			var model = await _db.Submissions
				.Where(s => s.GameId <= 0)
				.Select(s => new SubmissionsWithNoGamesModel(s.Id, s.Title))
				.ToListAsync();

			return View(model);
		}
	}
}
