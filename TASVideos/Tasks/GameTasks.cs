using System.Linq;
using System.Threading.Tasks;

using Microsoft.EntityFrameworkCore;

using TASVideos.Data;
using TASVideos.Models;

namespace TASVideos.Tasks
{
	public class GameTasks
	{
		private readonly ApplicationDbContext _db;

		public GameTasks(ApplicationDbContext db)
		{
			_db = db;
		}

		public async Task<GameViewModel> GetGameForDisplay(int id)
		{
			var game = await _db.Games
				.Include(g => g.System)
				.Include(g => g.GameGenres)
				.ThenInclude(gg => gg.Genre)
				.SingleOrDefaultAsync(g => g.Id == id);

			if (game != null)
			{
				var model = new GameViewModel
				{
					Id = game.Id,
					DisplayName = game.DisplayName,
					Abbreviation = game.Abbreviation,
					ScreenshotUrl = game.ScreenshotUrl,
					SystemCode = game.System.Code,
					Genres = game.GameGenres.Select(gg => gg.Genre.DisplayName)
				};

				return model;
			}

			return null;
		}
	}
}
