using System.Threading.Tasks;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

using TASVideos.Data;
using TASVideos.Data.Entity.Game;
using TASVideos.Services;
using TASVideos.Services.PublicationChain;

namespace TASVideos.Pages.Games
{
	[AllowAnonymous]
	public class PublicationHistoryModel : BasePageModel
	{
		private readonly ApplicationDbContext _db;
		private readonly IPublicationHistory _history;

		public PublicationHistoryModel(
			ApplicationDbContext db,
			IPublicationHistory history)
		{
			_history = history;
			_db = db;
		}

		[FromRoute]
		public int Id { get; set; }

		public PublicationHistoryGroup History { get; set; }
		public Game Game { get; set; }

		[FromQuery]
		public int? Highlight { get; set; }

		public async Task<IActionResult> OnGet()
		{
			Game = await _db.Games.SingleOrDefaultAsync(p => p.Id == Id);

			if (Game == null)
			{
				return NotFound();
			}

			History = await _history.ForGame(Id);

			return Page();
		}
	}
}
