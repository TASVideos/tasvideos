using TASVideos.Common;
using TASVideos.WikiEngine;

namespace TASVideos.WikiModules;

[WikiModule(ModuleNames.TabularMovieList)]
public class TabularMovieList(ApplicationDbContext db) : WikiViewComponent
{
	public List<TabularMovieEntry> Movies { get; set; } = [];

	public async Task<IViewComponentResult> InvokeAsync(int? limit, IList<string> tier)
	{
		var search = new TabularMovieListSearchModel();
		if (limit.HasValue)
		{
			search.Limit = limit.Value;
		}

		if (tier.Count > 0)
		{
			search.PublicationClasses = tier;
		}

		Movies = await MovieList(search);

		return View();
	}

	private async Task<List<TabularMovieEntry>> MovieList(TabularMovieListSearchModel searchCriteria)
	{
		var results = await db.Publications
			.Where(p => !searchCriteria.PublicationClasses.Any() || searchCriteria.PublicationClasses.Contains(p.PublicationClass!.Name))
			.ByMostRecent()
			.Take(searchCriteria.Limit)
			.Select(p => new TabularMovieEntry
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
					.Select(f => new TabularMovieEntry.ScreenshotFile
					{
						Path = f.Path,
						Description = f.Description
					})
					.First(),
				ObsoletedMovie = p.ObsoletedMovies
					.Select(o => new TabularMovieEntry.ObsoletedPublication
					{
						Id = o.Id,
						Frames = o.Frames,
						FrameRate = o.SystemFrameRate!.FrameRate
					})
					.FirstOrDefault()
			})
			.ToListAsync();

		return results;
	}

	public class TabularMovieListSearchModel
	{
		public int Limit { get; set; } = 10;
		public IEnumerable<string> PublicationClasses { get; set; } = [];
	}

	public class TabularMovieEntry : ITimeable
	{
		public int Id { get; init; }
		public DateTime CreateTimestamp { get; init; }

		public string System { get; init; } = "";
		public string Game { get; init; } = "";
		public string? Goal { get; init; }
		public IEnumerable<string>? Authors { get; init; }
		public string? AdditionalAuthors { get; init; }

		public ScreenshotFile Screenshot { get; init; } = new();

		public class ScreenshotFile
		{
			public string Path { get; init; } = "";
			public string? Description { get; set; }
		}

		public int Frames { get; init; }
		public double FrameRate { get; init; }

		public ObsoletedPublication? ObsoletedMovie { get; set; }

		public class ObsoletedPublication : ITimeable
		{
			public int Id { get; set; }
			public int Frames { get; init; }
			public double FrameRate { get; init; }
		}
	}
}
