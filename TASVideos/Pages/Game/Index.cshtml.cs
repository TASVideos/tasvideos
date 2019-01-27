using System.Linq;
using System.Threading.Tasks;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

using TASVideos.Data;
using TASVideos.Models;
using TASVideos.Services;

namespace TASVideos.Pages.Game
{
	[AllowAnonymous]
	public class IndexModel : BasePageModel
	{
		private readonly ApplicationDbContext _db;

		public IndexModel(
			ApplicationDbContext db,
			UserManager userManager)
			: base(userManager)
		{
			_db = db;
		}

		[FromRoute]
		public int Id { get; set; }

		[BindProperty]
		public GameDisplayModel Game { get; set; }

		public async Task OnGet()
		{
			Game = await _db.Games
				.Where(g => g.Id == Id)
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
