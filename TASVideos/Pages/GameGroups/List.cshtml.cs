using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using TASVideos.Data;

namespace TASVideos.Pages.GameGroups;

[AllowAnonymous]
public class ListModel : PageModel
{
	private readonly ApplicationDbContext _db;

	public ListModel(ApplicationDbContext db)
	{
		_db = db;
	}

	public IEnumerable<GroupListEntry> GameGroups { get; set; } = new List<GroupListEntry>();

	public async Task<IActionResult> OnGet()
	{
		GameGroups = await _db.GameGroups
			.Select(gg => new GroupListEntry(gg.Id, gg.Name))
			.ToListAsync();

		return Page();
	}

	public record GroupListEntry(int Id, string Name);
}
