using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TASVideos.Data;

namespace TASVideos.Pages.GameGroups;

[AllowAnonymous]
public class ListModel(ApplicationDbContext db) : BasePageModel
{
	public IEnumerable<GroupListEntry> GameGroups { get; set; } = new List<GroupListEntry>();

	public async Task<IActionResult> OnGet()
	{
		GameGroups = await db.GameGroups
			.Select(gg => new GroupListEntry(gg.Id, gg.Name))
			.ToListAsync();

		return Page();
	}

	public record GroupListEntry(int Id, string Name);
}
