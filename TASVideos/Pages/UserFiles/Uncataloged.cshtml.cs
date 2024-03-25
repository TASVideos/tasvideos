using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TASVideos.Data;
using TASVideos.Data.Entity;
using TASVideos.Pages.UserFiles.Models;

namespace TASVideos.Pages.UserFiles;

[AllowAnonymous]
public class Uncataloged(ApplicationDbContext db) : BasePageModel
{
	public List<UncatalogedViewModel> Files { get; set; } = [];

	[FromRoute]
	public int Id { get; set; }

	public async Task<IActionResult> OnGet()
	{
		Files = await db.UserFiles
			.ThatArePublic()
			.Where(uf => uf.GameId == null)
			.ToUnCatalogedModel()
			.ToListAsync();

		return Page();
	}
}
