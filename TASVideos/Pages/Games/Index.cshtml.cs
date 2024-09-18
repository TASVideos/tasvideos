using TASVideos.Data.Entity.Forum;
using TASVideos.Data.Entity.Game;
using TASVideos.WikiModules;

namespace TASVideos.Pages.Games;

[AllowAnonymous]
public class IndexModel(ApplicationDbContext db) : BasePageModel
{
	[FromRoute]
	public string Id { get; set; } = "";

	public int ParsedId => int.TryParse(Id, out var id) ? id : -2;

	public GameDisplay Game { get; set; } = new();
	public List<TabMiniMovieModel> Movies { get; set; } = [];
	public List<WatchFile> WatchFiles { get; set; } = [];
	public List<TopicEntry> Topics { get; set; } = [];

	public async Task<IActionResult> OnGet()
	{
		var query = db.Games.Select(g => new GameDisplay
		{
			Id = g.Id,
			DisplayName = g.DisplayName,
			Abbreviation = g.Abbreviation,
			Aliases = g.Aliases,
			ScreenshotUrl = g.ScreenshotUrl,
			GameResourcesPage = g.GameResourcesPage,
			Genres = g.GameGenres.Select(gg => gg.Genre!.DisplayName).ToList(),
			Versions = g.GameVersions.Select(gv => new GameDisplay.GameVersion(
				gv.Type,
				gv.Md5,
				gv.Sha1,
				gv.Name,
				gv.Region,
				gv.Version,
				gv.System!.Code,
				gv.TitleOverride)).ToList(),
			GameGroups = g.GameGroups.Select(gg => new GameDisplay.GameGroup(gg.GameGroupId, gg.GameGroup!.Name)).ToList(),
			PublicationCount = g.Publications.Count(p => p.ObsoletedById == null),
			ObsoletePublicationCount = g.Publications.Count(p => p.ObsoletedById != null),
			SubmissionCount = g.Submissions.Count,
			UserFilesCount = g.UserFiles.Count(uf => !uf.Hidden),
			PlaygroundGoals = g.Submissions
				.Where(s => s.Status == SubmissionStatus.Playground)
				.GroupBy(s => s.GameGoal)
				.OrderBy(gg => gg.Key!.DisplayName.Length)
				.Select(gg => new GoalEntry(
					gg.Key!.Id,
					gg.Key!.DisplayName,
					gg.OrderByDescending(ggs => ggs.Id)
						.Select(ggs => new SubmissionEntry(ggs.Id, ggs.Title))
						.ToList()))
				.ToList(),
		});

		query = ParsedId > -2
			? query.Where(g => g.Id == ParsedId)
			: query.Where(g => g.Abbreviation == Id);

		var game = await query.SingleOrDefaultAsync();

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
				.Select(f => new DisplayMiniMovie.MiniMovieModel.ScreenshotFile
				{
					Path = f.Path,
					Description = f.Description
				})
				.First(),
				OnlineWatchingUrl = p.PublicationUrls
				.First(u => u.Type == PublicationUrlType.Streaming).Url,
				GameTitle = p.GameVersion != null && p.GameVersion.TitleOverride != null ? p.GameVersion.TitleOverride : p.Game!.DisplayName
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
				new DisplayMiniMovie.MiniMovieModel
				{
					Id = m.Id,
					Title = m.Title,
					Goal = m.Goal,
					Screenshot = m.Screenshot,
					OnlineWatchingUrl = m.OnlineWatchingUrl
				}))
			.ToList();

		WatchFiles = await db.UserFiles
			.ForGame(Game.Id)
			.ThatArePublic()
			.Where(u => u.Type == "wch")
			.Select(u => new WatchFile(u.Id, u.FileName))
			.ToListAsync();

		Topics = await db.ForumTopics
			.ForGame(Game.Id)
			.Select(t => new TopicEntry(t.Id, t.Title))
			.ToListAsync();

		return Page();
	}

	public record WatchFile(long Id, string FileName);
	public record TopicEntry(int Id, string Title);
	public record GoalEntry(int Id, string Name, List<SubmissionEntry> Submissions);
	public record SubmissionEntry(int Id, string Title);

	public class GameDisplay
	{
		public int Id { get; init; }
		public string DisplayName { get; init; } = "";
		public string? Abbreviation { get; init; }
		public string? Aliases { get; init; }
		public string? ScreenshotUrl { get; init; }
		public string? GameResourcesPage { get; init; }
		public List<string> Genres { get; init; } = [];
		public List<GameVersion> Versions { get; init; } = [];
		public List<GameGroup> GameGroups { get; init; } = [];
		public List<GoalEntry> PlaygroundGoals { get; set; } = [];
		public int PublicationCount { get; init; }
		public int ObsoletePublicationCount { get; init; }
		public int SubmissionCount { get; init; }
		public int UserFilesCount { get; init; }

		public record GameVersion(
			VersionTypes Type,
			string? Md5,
			string? Sha1,
			string Name,
			string? Region,
			string? Version,
			string? SystemCode,
			string? TitleOverride);

		public record GameGroup(int Id, string Name);
	}

	public record TabMiniMovieModel(string TabTitleRegular, string TabTitleBold, DisplayMiniMovie.MiniMovieModel Movie);
}
