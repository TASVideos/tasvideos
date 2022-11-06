using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TASVideos.Data;
using TASVideos.Data.Entity;
using TASVideos.Pages.UserFiles.Models;

namespace TASVideos.Pages.UserFiles;

[AllowAnonymous]
public class Uncataloged : BasePageModel
{
	private readonly ApplicationDbContext _db;

	public Uncataloged(ApplicationDbContext db)
	{
		_db = db;
	}

	public IReadOnlyCollection<UncatalogedViewModel> Files { get; set; } = new List<UncatalogedViewModel>();

	[FromRoute]
	public int Id { get; set; }

	public async Task<IActionResult> OnGet()
	{
		Files = await _db.UserFiles
			.Where(uf => !uf.Hidden)
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
