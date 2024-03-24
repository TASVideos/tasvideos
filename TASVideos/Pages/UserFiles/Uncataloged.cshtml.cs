using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TASVideos.Data;
using TASVideos.Data.Entity;
using TASVideos.Pages.UserFiles.Models;

namespace TASVideos.Pages.UserFiles;

[AllowAnonymous]
public class Uncataloged(ApplicationDbContext db) : BasePageModel
{
	public IReadOnlyCollection<UncatalogedViewModel> Files { get; set; } = [];

	[FromRoute]
	public int Id { get; set; }

	public async Task<IActionResult> OnGet()
	{
		Files = await db.UserFiles
			.ThatArePublic()
			.Where(uf => uf.GameId == null)
			.Select(uf => new UncatalogedViewModel
			{
				Id = uf.Id,
				FileName = uf.FileName,
				SystemCode = uf.System != null ? uf.System.Code : null,
				UploadTimestamp = uf.UploadTimestamp,
				Author = uf.Author!.UserName
			})
			.ToListAsync();

		return Page();
	}
}
