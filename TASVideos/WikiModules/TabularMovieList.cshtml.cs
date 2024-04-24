using TASVideos.Common;
using TASVideos.WikiEngine;

namespace TASVideos.WikiModules;

[WikiModule(ModuleNames.TabularMovieList)]
public class TabularMovieList(ApplicationDbContext db) : WikiViewComponent
{
	public List<MovieEntry> Movies { get; set; } = [];

	public async Task<IViewComponentResult> InvokeAsync(int? limit)
	{
		Movies = await db.Publications
			.ByMostRecent()
			.Take(limit ?? 10)
			.Select(p => new MovieEntry
			{
				Id = p.Id,
				CreateTimestamp = p.CreateTimestamp,
				Frames = p.Frames,
				FrameRate = p.SystemFrameRate!.FrameRate,
				System = p.System!.Code,
				Game = p.GameVersion != null && !string.IsNullOrEmpty(p.GameVersion.TitleOverride) ? p.GameVersion.TitleOverride : p.Game!.DisplayName,
				Goal = p.GameGoal!.DisplayName,
				Authors = p.Authors.OrderBy(pa => pa.Ordinal).Select(pa => pa.Author!.UserName),
				AdditionalAuthors = p.AdditionalAuthors,
				Screenshot = p.Files
					.Where(f => f.Type == FileType.Screenshot)
					.Select(f => new MovieEntry.ScreenshotFile(f.Path, f.Description))
					.First()
			})
			.ToListAsync();

		return View();
	}

	public class MovieEntry : ITimeable
	{
		public int Id { get; init; }
		public DateTime CreateTimestamp { get; init; }
		public string System { get; init; } = "";
		public string Game { get; init; } = "";
		public string? Goal { get; init; }
		public IEnumerable<string>? Authors { get; init; }
		public string? AdditionalAuthors { get; init; }
		public int Frames { get; init; }
		public double FrameRate { get; init; }
		public ScreenshotFile Screenshot { get; init; } = null!;

		public record ScreenshotFile(string Path, string? Description);
	}
}
