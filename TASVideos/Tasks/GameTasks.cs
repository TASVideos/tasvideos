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

		public async Task<GameDisplayModel> GetGameForDisplay(int id)
		{
			return await _db.Games
				.Where(g => g.Id == id)
				.Select(g => new GameDisplayModel
				{
					Id = g.Id,
					DisplayName = g.DisplayName,
					Abbreviation = g.Abbreviation,
					ScreenshotUrl = g.ScreenshotUrl,
					SystemCode = g.System.Code,
					Genres = g.GameGenres
						.Select(gg => gg.Genre.DisplayName)
						.ToList()
				})
				.SingleOrDefaultAsync();
		}
	}
}
