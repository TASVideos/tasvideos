using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using TASVideos.Data;

namespace TASVideos.Pages.RamAddresses;

public class ListModel : PageModel
{
	private readonly ApplicationDbContext _db;

	public ListModel(ApplicationDbContext db)
	{
		_db = db;
	}

	public IEnumerable<RamAddressListEntry> Entries { get; set; } = new List<RamAddressListEntry>();

	public async Task<IActionResult> OnGet()
	{
		Entries = await _db.GameRamAddresses
			.Where(r => r.GameId.HasValue)
			.Select(r => new RamAddressListEntry(r.Game!.Id, r.Game.DisplayName, r.Game!.System!.Code))
			.Distinct()
			.ToListAsync();

		return Page();
	}

	public record RamAddressListEntry(int GameId, string GameName, string SystemCode);
}
