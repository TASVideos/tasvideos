using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;
using TASVideos.Data;
using TASVideos.Data.Entity.Game;

namespace TASVideos.Pages.Games.Versions;

public class ListModel(ApplicationDbContext db) : BasePageModel
{
	[FromRoute]
	public int GameId { get; set; }

	public VersionListModel Versions { get; set; } = new();

	public async Task<IActionResult> OnGet()
	{
		var roms = await db.Games
			.Where(g => g.Id == GameId)
			.Select(g => new VersionListModel
			{
				GameDisplayName = g.DisplayName,
				Versions = g.GameVersions
				.Select(r => new VersionListModel.VersionEntry
				{
					Id = r.Id,
					Name = r.Name,
					Md5 = r.Md5,
					Sha1 = r.Sha1,
					Version = r.Version,
					Region = r.Region,
					Type = r.Type,
					System = r.System!.Code,
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

	public class VersionListModel
	{
		[Display(Name = "Game")]
		public string GameDisplayName { get; init; } = "";

		public List<VersionEntry> Versions { get; init; } = [];

		public class VersionEntry
		{
			public int Id { get; init; }
			public string Name { get; init; } = "";
			public string? Md5 { get; init; }
			public string? Sha1 { get; init; }
			public string? Version { get; init; }
			public string? Region { get; init; }
			public VersionTypes Type { get; init; }
			public string System { get; init; } = "";

			[Display(Name = "Title Override")]
			public string? TitleOverride { get; init; }
		}
	}
}
