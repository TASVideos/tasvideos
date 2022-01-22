using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using TASVideos.Data;
using TASVideos.Data.Entity.Game;
using TASVideos.Pages.Games.Groups.Models;

namespace TASVideos.Pages.GamesGroups
{
	public class IndexModel : PageModel
	{
		private readonly ApplicationDbContext _db;

		[FromRoute]
		public int Id { get; set; }

		public IEnumerable<GameListEntry> Games { get; set; } = new List<GameListEntry>();

		public string Name { get; set; } = "";

		public IndexModel(ApplicationDbContext db)
		{
			_db = db;
		}

		public async Task<IActionResult> OnGet()
		{
			var gameGroup = await _db.GameGroups.SingleOrDefaultAsync(gg => gg.Id == Id);

			if (gameGroup == null)
			{
				return NotFound();
			}

			Name = gameGroup.Name;

			Games = await _db.Games
				.ForGroup(Id)
				.Select(g => new GameListEntry
				{
					Id = g.Id,
					Name = g.DisplayName,
					SystemCode = g.System!.Code,
					PublicationCount = g.Publications.Count,
					SubmissionsCount = g.Submissions.Count,
					GameResourcesPage = g.GameResourcesPage
				})
				.ToListAsync();

			return Page();
		}
	}
}
