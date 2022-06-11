using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TASVideos.Data;
using TASVideos.Pages.Games.Versions.Models;

namespace TASVideos.Pages.Games.Versions;

public class ListModel : BasePageModel
{
	private readonly ApplicationDbContext _db;

	public ListModel(ApplicationDbContext db)
	{
		_db = db;
	}

	[FromRoute]
	public int GameId { get; set; }

	public VersionListModel Versions { get; set; } = new();

	public async Task<IActionResult> OnGet()
	{
		var roms = await _db.Games
			.Where(g => g.Id == GameId)
			.Select(g => new VersionListModel
			{
				GameDisplayName = g.DisplayName,
				Versions = g.GameVersions
				.Select(r => new VersionListModel.VersionEntry
				{
					Id = r.Id,
					DisplayName = r.Name,
					Md5 = r.Md5,
					Sha1 = r.Sha1,
					Version = r.Version,
					Region = r.Region,
					VersionType = r.Type,
					SystemCode = r.System!.Code,
					TitleOverride = r.TitleOverride,
				})
				.ToList()
			})
			.SingleOrDefaultAsync();

		if (roms is null)
		{
			return NotFound();
		}

		Versions = roms;
		return Page();
	}
}
