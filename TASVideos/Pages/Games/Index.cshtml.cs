using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TASVideos.Data;
using TASVideos.Data.Entity;
using TASVideos.Data.Entity.Forum;
using TASVideos.Pages.Games.Models;
using TASVideos.ViewComponents;

namespace TASVideos.Pages.Games;

[AllowAnonymous]
public class IndexModel(ApplicationDbContext db) : BasePageModel
{
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
		var query = db.Games.ToGameDisplayModel();

		query = ParsedId > 0
			? query.Where(g => g.Id == ParsedId)
			: query.Where(g => g.Abbreviation == Id);

		var game = await query
			.SingleOrDefaultAsync();

		if (game is null)
		{
			return NotFound();
		}

		Game = game;
		var movies = await db.Publications
			.Where(p => p.GameId == Game.Id && p.ObsoletedById == null)
			.OrderBy(p => p.GameGoal!.DisplayName == "baseline" ? -1 : p.GameGoal!.DisplayName.Length)
			.ThenBy(p => p.Frames)
			.Select(p => new
			{
				p.Id,
				p.Title,
				Goal = p.GameGoal!.DisplayName,
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
				movies.Count(mm => mm.Goal == m.Goal) > 1
					? m.GameTitle
					: m.Goal == "baseline"
					? "(baseline)"
					: "",
				m.Goal == "baseline"
					? ""
					: m.Goal,
				new MiniMovieModel
				{
					Id = m.Id,
					Title = m.Title,
					Goal = m.Goal,
					Screenshot = m.Screenshot,
					OnlineWatchingUrl = m.OnlineWatchingUrl,
				}))
			.ToList();

		WatchFiles = await db.UserFiles
			.ForGame(Game.Id)
			.Where(u => !u.Hidden)
			.Where(u => u.Type == "wch")
			.Select(u => new WatchFile(u.Id, u.FileName))
			.ToListAsync();

		Topics = await db.ForumTopics
			.ForGame(Game.Id)
			.Select(t => new TopicEntry(t.Id, t.Title))
			.ToListAsync();

		return Page();
	}

	// TODO: move me
	public record WatchFile(long Id, string FileName);
	public record TopicEntry(int Id, string Title);
}
