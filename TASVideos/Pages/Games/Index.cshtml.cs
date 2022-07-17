using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TASVideos.Data;
using TASVideos.Data.Entity;
using TASVideos.Pages.Games.Models;
using TASVideos.ViewComponents;

namespace TASVideos.Pages.Games;

[AllowAnonymous]
public class IndexModel : BasePageModel
{
	private readonly ApplicationDbContext _db;

	public IndexModel(ApplicationDbContext db)
	{
		_db = db;
	}

	[FromRoute]
	public string Id { get; set; } = "";

	public int ParsedId => int.TryParse(Id, out var id) ? id : -1;

	public GameDisplayModel Game { get; set; } = new();

	public IEnumerable<MiniMovieModel> Movies { get; set; } = new List<MiniMovieModel>();

	public IReadOnlyCollection<WatchFile> WatchFiles { get; set; } = new List<WatchFile>();

	public async Task<IActionResult> OnGet()
	{
		var query = _db.Games.ToGameDisplayModel();

		query = ParsedId > 0
			? query.Where(g => g.Id == ParsedId)
			: query.Where(g => g.Abbreviation == Id);

		// TODO: abbreviations need to be unique, then we can use Single here
		var game = await query
			.FirstOrDefaultAsync();

		if (game is null)
		{
			return NotFound();
		}

		Game = game;
		Movies = await _db.Publications
			.Where(p => p.GameId == Game.Id && p.ObsoletedById == null)
			.OrderBy(p => p.Branch == null ? -1 : p.Branch.Length)
			.ThenBy(p => p.Frames)
			.ToMiniMovieModel()
			.ToListAsync();

		WatchFiles = await _db.UserFiles
			.ForGame(Game.Id)
			.FilterByHidden(false)
			.Where(u => u.Type == "wch")
			.Select(u => new WatchFile(u.Id, u.FileName))
			.ToListAsync();

		return Page();
	}

	// TODO: move me
	public record WatchFile(long Id, string FileName);
}
