using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TASVideos.Data;
using TASVideos.Data.Entity;
using TASVideos.Data.Entity.Forum;
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

	public record TabMiniMovieModel(string TabTitleRegular, string TabTitleBold, MiniMovieModel Movie);

	public List<TabMiniMovieModel> Movies { get; set; } = new List<TabMiniMovieModel>();

	public IReadOnlyCollection<WatchFile> WatchFiles { get; set; } = new List<WatchFile>();

	public IReadOnlyCollection<TopicEntry> Topics { get; set; } = new List<TopicEntry>();

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
		var movies = await _db.Publications
			.Where(p => p.GameId == Game.Id && p.ObsoletedById == null)
			.OrderBy(p => p.Branch == null ? -1 : p.Branch.Length)
			.ThenBy(p => p.Frames)
			.Select(p => new
			{
				p.Id,
				p.Title,
				Branch = p.Branch ?? "",
				Screenshot = p.Files
				.Where(f => f.Type == FileType.Screenshot)
				.Select(f => new MiniMovieModel.ScreenshotFile
				{
					Path = f.Path,
					Description = f.Description
				})
				.First(),
				OnlineWatchingUrl = p.PublicationUrls
				.First(u => u.Type == PublicationUrlType.Streaming).Url,
				GameTitle = (p.GameVersion != null && p.GameVersion.TitleOverride != null) ? p.GameVersion.TitleOverride : p.Game!.DisplayName,
			})
			.ToListAsync();

		Movies = movies
			.Select(m => new TabMiniMovieModel(
				movies.Where(mm => mm.Branch == m.Branch).Count() > 1 ? m.GameTitle : string.IsNullOrEmpty(m.Branch) ? "(baseline)" : "",
				m.Branch,
				new MiniMovieModel
				{
					Id = m.Id,
					Title = m.Title,
					Branch = m.Branch,
					Screenshot = m.Screenshot,
					OnlineWatchingUrl = m.OnlineWatchingUrl,
				}))
			.ToList();

		WatchFiles = await _db.UserFiles
			.ForGame(Game.Id)
			.Where(u => !u.Hidden)
			.Where(u => u.Type == "wch")
			.Select(u => new WatchFile(u.Id, u.FileName))
			.ToListAsync();

		Topics = await _db.ForumTopics
			.ForGame(Game.Id)
			.Select(t => new TopicEntry(t.Id, t.Title))
			.ToListAsync();

		return Page();
	}

	// TODO: move me
	public record WatchFile(long Id, string FileName);
	public record TopicEntry(int Id, string Title);
}
