using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using TASVideos.Data;
using TASVideos.Data.Entity.Game;

namespace TASVideos.Pages.RamAddresses;

public class IndexModel : PageModel
{
	private readonly ApplicationDbContext _db;

	public IndexModel(ApplicationDbContext db)
	{
		_db = db;
	}

	[FromRoute]
	public int Id { get; set; }

	public string GameName { get; set; } = "";
	public string SystemCode { get; set; } = "";

	public IEnumerable<GameRamAddress> Addresses { get; set; } = new List<GameRamAddress>();

	public async Task<IActionResult> OnGet()
	{
		var game = await _db.Games
			.Include(g => g.System)
			.Where(g => g.Id == Id)
			.SingleOrDefaultAsync();

		if (game is null)
		{
			return NotFound();
		}

		GameName = game.DisplayName;
		SystemCode = game.System!.Code;

		Addresses = await _db.GameRamAddresses
			.Include(r => r.GameRamAddressDomain)
			.Where(r => r.GameId == Id)
			.ToListAsync();

		return Page();
	}
}
