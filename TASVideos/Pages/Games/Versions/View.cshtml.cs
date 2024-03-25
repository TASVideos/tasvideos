using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TASVideos.Data;
using TASVideos.Data.Entity.Game;

namespace TASVideos.Pages.Games.Versions;

[AllowAnonymous]
public class ViewModel(ApplicationDbContext db) : BasePageModel
{
	[FromRoute]
	public int GameId { get; set; }

	[FromRoute]
	public int Id { get; set; }

	[BindProperty]
	public VersionDisplayModel Version { get; set; } = null!;

	[BindProperty]
	[Display(Name = "Game")]
	public string GameName { get; set; } = "";

	public bool CanDelete { get; set; }

	public async Task<IActionResult> OnGet()
	{
		var game = await db.Games.SingleOrDefaultAsync(g => g.Id == GameId);

		if (game is null)
		{
			return NotFound();
		}

		GameName = game.DisplayName;

		var version = await db.GameVersions
			.Where(r => r.Id == Id && r.Game!.Id == GameId)
			.Select(v => new VersionDisplayModel(
				v.System!.Code,
				v.Name,
				v.Md5,
				v.Sha1,
				v.Version,
				v.Region,
				v.Type,
				v.TitleOverride,
				game.Id,
				game.DisplayName))
			.SingleOrDefaultAsync();

		if (version is null)
		{
			return NotFound();
		}

		Version = version;

		return Page();
	}

	public record VersionDisplayModel(
		string SystemCode,
		string Name,
		string? Md5,
		string? Sha1,
		string? Version,
		string? Region,
		VersionTypes Type,
		string? TitleOverride,
		int GameId,
		string GameName);
}
